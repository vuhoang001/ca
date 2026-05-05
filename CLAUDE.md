# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Centralized authentication / authorization service. ASP.NET Core 10 Minimal API + EF Core (SQL Server) + MediatR + FluentValidation + JWT. The full domain/architecture write-up lives in `docs/architecture.md` — read it before making non-trivial changes; this file only covers what you cannot derive from the code.

## Common commands

SDK is pinned in `global.json` to .NET 10 (`10.0.201`). Package versions are managed centrally in `Directory.Packages.props` (do **not** add `Version="..."` on `<PackageReference>` entries — add a `<PackageVersion>` there instead).

```bash
# Build everything
dotnet build AuthSystem.sln

# Run the API (defaults to http://localhost:8080 in Docker; http://localhost:5xxx locally)
dotnet run --project Api/Api.csproj

# Run via .NET Aspire host (spins up SQL Server + API together)
dotnet run --project AppHost/AppHost.csproj

# Tests
dotnet test Tests/Domain.UnitTests/Domain.UnitTests.csproj
dotnet test Tests/Application.UnitTests/Application.UnitTests.csproj
dotnet test Tests/Integration.Tests/Integration.Tests.csproj    # uses Testcontainers.MsSql; needs Docker
dotnet test --filter "FullyQualifiedName~AuthCommandHandlerTests"

# Format check (CI fails the build on diffs — run before pushing)
dotnet format AuthSystem.sln --verify-no-changes --severity warn
dotnet format AuthSystem.sln                       # apply fixes

# EF Core migrations (Infrastructure is the migrations assembly, Api is the startup project)
dotnet ef migrations add <Name> --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj
dotnet ef database update          --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj

# Local SQL Server + API via docker-compose (dev)
docker compose -f docker-compose.yaml up --build
```

## Architecture — what you can't see by listing files

### Layering and the live wiring

`Api → Application → Domain` and `Api → Infrastructure → {Application, Domain, Shared}`. The composition root is `Api/Program.cs`, which calls `services.AddApplication()` (`Application/Src/DependencyInjection.cs`) and `services.AddInfrastructure(configuration)` (`Infrastructure/Src/DependencyInjection.cs`). Adding a new service generally means registering it in one of these two files.

### Only `Src/` and `Migrations/` are compiled

Every project sets `EnableDefaultCompileItems=false` and explicitly compiles `Src/**/*.cs` (Infrastructure also adds `Migrations/**/*.cs`). Top-level files like `Application/Extensions.cs`, `Application/IBasketMarker.cs`, `Application/Auth/`, `Domain/Entities/Auth.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Extensions.cs`, `Shared/IDomainEventDispatcher.cs` are **not built** — they are dormant scaffolding from an alternate design. The real `AppDbContext` is `Infrastructure/Src/Persistence/AppDbContext.cs`; the real DI is `Application/Src/DependencyInjection.cs` and `Infrastructure/Src/DependencyInjection.cs`. When adding code, put it under `Src/` or it will silently never compile.

### Endpoint registration

Endpoints are minimal-API modules implementing `IEndpointModule` in `Api/Src/Endpoints/V1/*.cs`. New endpoint files must be added to the `modules` array in `Api/Src/Endpoints/EndpointExtensions.cs` — there's no auto-discovery. All v1 endpoints are mounted under `/api/v1` (see `Api/Src/Extensions/ApiVersions.cs`).

### Authorization model — important defaults

`Infrastructure/Src/DependencyInjection.cs` sets a **fallback policy that requires authenticated users**. Any new endpoint is authenticated by default; to expose it anonymously you must explicitly call `.AllowAnonymous()`. Stricter checks use `.RequireAuthorization(p => p.RequirePermission(PermissionCodes.X))` — permission codes live in `Application/Src/Common/PermissionCodes.cs` and are matched against the `permissions` claim in the JWT (case-insensitive), via `PermissionAuthorizationHandler`.

Implication: **permissions/roles are embedded in the JWT at login/refresh time**. Granting or revoking a permission in the DB does not affect already-issued tokens until the user re-authenticates or refreshes. Don't write code that assumes DB state and token state are in sync.

### Request pipeline (MediatR)

Handlers are dispatched through MediatR with two pipeline behaviors registered in order: `ValidationBehavior` (runs FluentValidation, throws `ValidationException`) → `LoggingBehavior`. Place commands/queries under `Application/Src/Features/<Area>/{Commands,Queries}/` and their FluentValidation validators in the same file or alongside. They are auto-discovered by assembly scan, no manual registration needed.

### Error handling and response shape

`Api/Src/Middleware/GlobalExceptionHandler.cs` maps `ValidationException` → 400, `AppException` (in `Shared/Src/Exceptions`) → its declared status, everything else → 500, all as `ProblemDetails`. Successful responses are wrapped in `Shared/Src/Results/ApiEnvelope.cs` (`{ data, message? }`) — keep this pattern when adding endpoints.

### Persistence and audit fields

`AppDbContext` overrides `SaveChangesAsync` to stamp `CreatedAtUtc/CreatedBy/LastModifiedAtUtc/LastModifiedBy` for any `AuditableEntity`, sourced from `IDateTimeProvider` and `ICurrentUserContext` (resolved from `HttpContext`). Inherit new auditable entities from `AuditableEntity` and they'll be stamped automatically. Entity → table mappings live in `Infrastructure/Src/Configurations/`. Migrations target SQL Server via `ConnectionStrings:AuthDb`.

### Token mechanics — quick reminders

- Access token: stateless JWT. Revocation is implemented as a `jti` denylist (`revoked_access_tokens`); `JwtBearerEvents.OnTokenValidated` rejects denylisted `jti`s.
- Refresh token: 64-byte random value returned to the client; only the **SHA-256 hash** is stored. Rotation revokes the old row with `RevocationReason=Rotated` and a `ReplacedByTokenId` link.
- Role/permission assignment endpoints are **replace-style**, not additive — passing `[a, b]` removes anything not in that list.

### Seeding

On startup `Api/Program.cs` calls `InitializeDatabaseAsync()` which runs `Database.MigrateAsync()` then `DbSeeder.SeedAsync()` (`Infrastructure/Src/Seed/`). Seeding is gated on "no users exist" — it will not re-run on an existing DB. Default credentials come from the `Seed` config section (admin/admin@local.dev/Admin@123456 by default).

### Rate limiting

Two named limiters in `Program.cs`: `auth` (10/min, on register/login/refresh) and `default` (60/min, on authenticated business endpoints). Apply with `.RequireRateLimiting("auth"|"default")`.

## Configuration

`Api/appsettings.json` is checked in with **placeholder secrets** (e.g. `change-this-super-secret-key-...`). Real secrets come from environment variables in Docker or user-secrets locally. Keys that matter: `ConnectionStrings:AuthDb`, `Jwt:{Issuer,Audience,SigningKey,AccessTokenMinutes,RefreshTokenDays}`, `Seed:{AdminEmail,AdminUserName,AdminPassword,DefaultTenantName,DefaultTenantSlug}`.

## CI/CD

`.github/workflows/ci.yaml` runs on push/PR to `main` and `develop`: restore, build (Release), `dotnet format --verify-no-changes`, Domain + Application unit tests, then publish artifact. Integration tests are defined but currently commented out in CI. **Note**: the workflow's `DOTNET_VERSION` is `8.0.x` but the projects target `net10.0` (per `global.json`); rely on `setup-dotnet` resolving the SDK from `global.json` rather than the env var if you touch this.

`.github/workflows/cd.yaml` triggers on successful CI completion for `main`/`develop` and pushes a Docker image to Docker Hub at `${{ vars.DOCKERHUB_USERNAME }}/auth-system`, tagged `latest` (main), `staging` (develop), and `<branch>-<run>-<short-sha>`. Production deploy uses `docker-compose.prod.yaml` with secrets injected via `.env`.
