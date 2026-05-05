# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

**MasterData Template** — production-ready Clean Architecture starter for masterdata microservices. ASP.NET Core 10 Minimal API + EF Core (SQL Server) + MediatR + FluentValidation. Authentication/authorization is fully delegated to **Keycloak** (OIDC/JWT) — no internal auth system.

Full documentation lives in `docs/` (Docusaurus). This file covers non-obvious wiring that cannot be derived from the code.

## Common commands

SDK is pinned in `global.json` to .NET 10 (`10.0.201`). Package versions are managed centrally in `Directory.Packages.props` (do **not** add `Version="..."` on `<PackageReference>` entries — add a `<PackageVersion>` there instead).

```bash
# Build everything
dotnet build MasterData.Template.sln

# Run the API (http://localhost:5xxx locally)
dotnet run --project Api/Api.csproj

# Run via .NET Aspire host (spins up SQL Server + Keycloak + API)
dotnet run --project AppHost/AppHost.csproj

# Dev dependencies only via docker-compose
docker compose up -d mssql keycloak

# Tests
dotnet test Tests/Domain.UnitTests/Domain.UnitTests.csproj
dotnet test Tests/Application.UnitTests/Application.UnitTests.csproj
dotnet test Tests/Integration.Tests/Integration.Tests.csproj    # Testcontainers.MsSql; needs Docker

# Format check (CI fails on diffs — run before pushing)
dotnet format MasterData.Template.sln --verify-no-changes --severity warn
dotnet format MasterData.Template.sln

# EF Core migrations
dotnet ef migrations add <Name> --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj
dotnet ef database update       --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj
```

## Architecture — what you can't see by listing files

### Layering and the live wiring

`Api → Application → Domain` and `Api → Infrastructure → {Application, Domain, Shared}`. The composition root is `Api/Program.cs`, which calls `services.AddApplication()` (`Application/Src/DependencyInjection.cs`) and `services.AddInfrastructure(configuration)` (`Infrastructure/Src/DependencyInjection.cs`). Adding a new service means registering it in one of these two files.

### Only `Src/` and `Migrations/` are compiled

Every project sets `EnableDefaultCompileItems=false` and explicitly compiles `Src/**/*.cs` (Infrastructure also adds `Migrations/**/*.cs`). When adding code, put it under `Src/` or it will silently never compile.

### Endpoint registration

Endpoints are minimal-API modules implementing `IEndpointModule` in `Api/Src/Endpoints/V1/*.cs`. New endpoint files **must** be added to the `modules` array in `Api/Src/Endpoints/EndpointExtensions.cs` — there is no auto-discovery. All v1 endpoints mount under `/api/v1`.

### Authorization model — Keycloak is the only source of truth

`Infrastructure/Src/DependencyInjection.cs` sets a **fallback policy that requires authenticated users**. Any new endpoint is authenticated by default; to expose it anonymously call `.AllowAnonymous()` explicitly.

**No database permission table. No internal RBAC.** Roles come exclusively from the Keycloak JWT:
- `realm_access.roles` → parsed by `Infrastructure/Src/Keycloak/KeycloakClaimsTransformation.cs` → added as `ClaimTypes.Role`
- `resource_access.{clientId}.roles` → same transformation

Use `.RequireAuthorization(p => p.RequireRole("masterdata-writer", "admin"))` on endpoints.

Granting/revoking a role in Keycloak takes effect on the user's **next token refresh** — no DB sync needed.

### Keycloak JWT claims used

| Claim | Maps to |
|---|---|
| `sub` | `ICurrentUserContext.UserId` (Guid) |
| `preferred_username` | `ICurrentUserContext.Username` |
| `email` | `ICurrentUserContext.Email`, audit `CreatedBy` |
| `realm_access.roles` | `ClaimTypes.Role` via `KeycloakClaimsTransformation` |
| `tenant_id` (custom) | `ICurrentUserContext.TenantId` |

### Request pipeline (MediatR)

Two pipeline behaviors in order: `ValidationBehavior` (FluentValidation, throws `ValidationException`) → `LoggingBehavior`. Place commands/queries under `Application/Src/Features/<Area>/{Commands,Queries}/`. Auto-discovered by assembly scan.

### Error handling and response shape

`Api/Src/Middleware/GlobalExceptionHandler.cs` maps `ValidationException` → 400, `AppException` subtypes → their declared status, everything else → 500, all as `ProblemDetails`. Successful responses are wrapped in `Shared/Src/Results/ApiEnvelope<T>` (`{ data, message? }`).

### Persistence and audit fields

`AppDbContext.SaveChangesAsync` stamps `CreatedAtUtc/CreatedBy/LastModifiedAtUtc/LastModifiedBy` for any `AuditableEntity`, sourced from `IDateTimeProvider` and `ICurrentUserContext.Email`. Inherit new entities from `AuditableEntity`. Entity → table mappings live in `Infrastructure/Src/Configurations/ModelConfigurations.cs`. Migrations target SQL Server via `ConnectionStrings:MasterDataDb`.

### No seeding on startup

`InitializeDatabaseAsync` only runs `Database.MigrateAsync()`. Default users and roles come from Keycloak's `keycloak/realm-export.json` imported on first startup.

### Rate limiting

One named limiter: `"default"` (60 req/min). Apply with `.RequireRateLimiting("default")` on all business endpoints.

## Configuration

Key config entries in `Api/appsettings.json`:

```json
{
  "ConnectionStrings": { "MasterDataDb": "..." },
  "Keycloak": {
    "Authority": "http://localhost:8181/realms/masterdata",
    "Audience": "masterdata-api",
    "ClientId": "masterdata-api",
    "RequireHttpsMetadata": false
  }
}
```

Real secrets come from environment variables (Docker) or user-secrets (local). `RequireHttpsMetadata` must be `true` in production.

## Keycloak local dev

```bash
# Start Keycloak (imports realm-export.json automatically)
docker compose up -d keycloak

# Get a token
curl -s -X POST http://localhost:8181/realms/masterdata/protocol/openid-connect/token \
  -d "grant_type=password&client_id=masterdata-api&client_secret=masterdata-api-secret-change-in-prod&username=admin&password=Admin@123456" \
  | jq .access_token
```

## CI/CD

`ci.yaml` runs on push/PR to `main`/`develop`: restore → build Release → format check → Domain + Application unit tests → publish artifact. Solution file: `MasterData.Template.sln`.

`cd.yaml` triggers on successful CI and pushes Docker image to `${{ vars.DOCKERHUB_USERNAME }}/masterdata-template`, tagged `latest` (main), `staging` (develop), `<branch>-<run>-<sha>`.
