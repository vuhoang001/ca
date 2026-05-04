# Auth Service Documentation

## 1. Purpose

This repository implements a centralized authentication and authorization service on ASP.NET Core Minimal API.

Current scope in code:

- User registration with email/password
- Login and JWT access token issuance
- Refresh token rotation
- Access-token and refresh-token revocation
- Role management
- Permission management
- Assign roles to users
- Assign permissions to roles
- Check whether a user has a permission
- Audit logging for security-sensitive actions

The architecture follows a layered style:

- `Auth.Api`: HTTP entrypoint, middleware, endpoint mapping, Swagger, rate limiting
- `Auth.Application`: use cases, commands/queries, validation, MediatR pipeline
- `Auth.Domain`: entities, enums, domain event definitions
- `Auth.Infrastructure`: EF Core persistence, repositories, JWT, authorization policy handler, seeding, runtime services
- `Auth.Shared`: base abstractions and common primitives
- `Auth.AppHost`: Aspire host shell, currently minimal
- `Auth.ServiceDefaults`: present in solution but not currently wired into `Auth.Api`

## 2. Solution Structure

```text
Auth.Api
  Program.cs
  Src/Endpoints
  Src/Extensions
  Src/Middleware

Auth.Application
  Src/Abstractions
  Src/Common
  Src/Contracts
  Src/Features
    Auth
    Permissions
    Roles
    Users

Auth.Domain
  Src/Entities
  Src/Enums
  Src/Events

Auth.Infrastructure
  Src/Authentication
  Src/Auditing
  Src/Configurations
  Src/Persistence
  Src/Repositories
  Src/Seed
  Src/Services
  Src/Options
  Migrations

Auth.Shared
  Src/Abstractions
  Src/Exceptions
  Src/Kernel
  Src/Results
```

## 3. Runtime Flow

### 3.1 Startup

`Auth.Api/Program.cs` does the following:

1. Builds the web host.
2. Configures Serilog from configuration.
3. Registers exception handling and problem details.
4. Registers application services with `AddApplication()`.
5. Registers infrastructure services with `AddInfrastructure(configuration)`.
6. Adds Swagger/OpenAPI.
7. Adds fixed-window rate limiting.
8. Builds the app pipeline.
9. Enables request logging, exception handling, rate limiting, Swagger, authentication, authorization.
10. Runs EF Core migration and data seeding via `InitializeDatabaseAsync()`.
11. Maps all API v1 endpoints under `/api/v1`.

### 3.2 Request pipeline

For a typical request, the execution path is:

1. ASP.NET Core receives the HTTP request.
2. Rate limiter checks the configured limiter.
3. Authentication validates the JWT bearer token when required.
4. Authorization checks fallback policy and permission requirements.
5. Minimal API endpoint converts request body/route data to a command/query.
6. MediatR sends the request into the application layer.
7. `ValidationBehavior` runs FluentValidation validators.
8. `LoggingBehavior` logs the request name and payload.
9. The handler orchestrates repository/domain/service calls.
10. `AppDbContext.SaveChangesAsync()` stamps audit fields.
11. `AuditService` persists audit logs for relevant operations.
12. Endpoint wraps successful output in `ApiEnvelope<T>`.
13. `GlobalExceptionHandler` converts known exceptions to `ProblemDetails`.

## 4. API Surface

All business endpoints are mapped under `/api/v1`.

### 4.1 Health

- `GET /api/v1/health`
- Anonymous
- Returns `{ data: { status: "ok" } }`

### 4.2 Auth endpoints

Group: `/api/v1/auth`

- `POST /register`
  - Anonymous
  - Rate limit: `auth`
  - Body: `email`, `userName`, `password`, optional `tenantId`
  - Output: registered user info

- `POST /login`
  - Anonymous
  - Rate limit: `auth`
  - Body: `email`, `password`, optional `clientId`
  - Output: access token, refresh token, token expiry timestamps, user profile with roles and permissions

- `POST /refresh-token`
  - Anonymous
  - Rate limit: `auth`
  - Body: `refreshToken`, optional `clientId`
  - Output: new access token and rotated refresh token

- `POST /change-password`
  - Authenticated
  - Rate limit: `default`
  - Body: `currentPassword`, `newPassword`
  - Output: `204 No Content`

- `POST /revoke`
  - Authenticated
  - Rate limit: `default`
  - Permission required: `auth.tokens.revoke`
  - Body: optional `refreshToken`, optional `accessToken`, `reason`
  - Output: `204 No Content`

### 4.3 Role endpoints

Group: `/api/v1/roles`

- All endpoints require authentication and `default` rate limit.

- `GET /`
  - Permission required: `auth.roles.read`
  - Optional query: `tenantId`

- `POST /`
  - Permission required: `auth.roles.manage`
  - Body: `name`, `description`, optional `tenantId`, optional `isSystem`

- `PUT /{roleId}`
  - Permission required: `auth.roles.manage`
  - Body: `name`, `description`, `isActive`

- `DELETE /{roleId}`
  - Permission required: `auth.roles.manage`

- `POST /{roleId}/permissions`
  - Permission required: `auth.roles.manage`
  - Body: `permissionIds`
  - Behavior: replaces the effective permission set of the role to match the provided list

### 4.4 Permission endpoints

Group: `/api/v1/permissions`

- All endpoints require authentication and `default` rate limit.

- `GET /`
  - Permission required: `auth.permissions.read`

- `POST /`
  - Permission required: `auth.permissions.manage`
  - Body: `code`, `name`, `resource`, `action`, optional `description`

- `PUT /{permissionId}`
  - Permission required: `auth.permissions.manage`
  - Body: `name`, `resource`, `action`, `description`, `isActive`

- `DELETE /{permissionId}`
  - Permission required: `auth.permissions.manage`

### 4.5 User endpoints

Group: `/api/v1/users`

- All endpoints require authentication and `default` rate limit.

- `POST /{userId}/roles`
  - Permission required: `auth.users.manage`
  - Body: `roleIds`
  - Behavior: replaces the effective role set of the user to match the provided list

- `GET /{userId}/permissions/{permissionCode}`
  - Permission required: `auth.users.read`
  - Output: whether the target user currently has the permission

## 5. Permission Model

The system uses RBAC with direct permission claims embedded into JWTs at login/refresh time.

Built-in permission codes live in `Auth.Application/Src/Common/PermissionCodes.cs`:

- `auth.roles.read`
- `auth.roles.manage`
- `auth.permissions.read`
- `auth.permissions.manage`
- `auth.users.read`
- `auth.users.manage`
- `auth.tokens.revoke`

Authorization behavior:

- The app sets a fallback authorization policy requiring authenticated users by default.
- Endpoints add stricter permission checks with `RequirePermission(...)`.
- `PermissionAuthorizationHandler` reads the JWT claim type `permissions`.
- A request succeeds if the required permission exists in the token claims, case-insensitive.

Important implication:

- Permission changes in the database do not affect already-issued access tokens until the next login or refresh.

## 6. Authentication and Token Design

### 6.1 Access token

`JwtTokenService.GenerateTokens()` creates a signed JWT containing:

- `sub`: user id
- `email`
- `preferred_username`
- `jti`: unique token identifier
- `ClaimTypes.NameIdentifier`
- `ClaimTypes.Email`
- optional `tenant_id`
- optional `client_id`
- repeated role claims using `ClaimTypes.Role`
- repeated permission claims using claim type `permissions`

Validation uses:

- issuer
- audience
- symmetric signing key
- expiration
- signing key validation
- `ClockSkew = 30 seconds`

Additionally, `JwtBearerEvents.OnTokenValidated` checks whether the token `jti` is present in `revoked_access_tokens` and still active.

### 6.2 Refresh token

Refresh tokens are random 64-byte values encoded as Base64.

Storage model:

- plaintext refresh token is returned only to the client
- database stores only SHA-256 hash (`TokenHash`)
- refresh token records keep `UserId`, optional `ClientAppId`, `JwtId`, expiry, device/client metadata, revocation fields

Rotation model:

1. Client sends refresh token.
2. Service hashes the token.
3. Service loads the current `RefreshToken` row.
4. Service rejects inactive/revoked/expired tokens.
5. Service generates a new access token and a new refresh token.
6. Service inserts the new refresh token row.
7. Service revokes the old refresh token with reason `Rotated` and stores `ReplacedByTokenId`.

### 6.3 Token revocation

Refresh token revocation:

- Hash incoming refresh token
- Find database row
- Set `RevokedAtUtc` and `RevocationReason`

Access token revocation:

- Parse access token without lifetime validation
- Extract `jti`, `userId`, `exp`
- Insert into `revoked_access_tokens` if no active blacklist row exists

## 7. Domain Model

### 7.1 Shared base classes

`Entity`

- `Id`

`AuditableEntity`

- `CreatedAtUtc`
- `CreatedBy`
- `LastModifiedAtUtc`
- `LastModifiedBy`

These are filled in `AppDbContext.ApplyAuditFields()`.

### 7.2 Entities

`Tenant`

- `Name`
- `Slug`
- `IsActive`

`User`

- `TenantId`
- `UserName`
- `NormalizedUserName`
- `Email`
- `NormalizedEmail`
- `PasswordHash`
- `SecurityStamp`
- `EmailConfirmed`
- `Status`
- `LastLoginAtUtc`
- navigation: `UserRoles`, `RefreshTokens`

Behavior:

- constructor normalizes email/user name
- raises `UserRegisteredDomainEvent`
- `SetPassword()` updates hash and security stamp
- `MarkLogin()` stores last login time
- `Activate()`, `Disable()`, `ConfirmEmail()`

`Role`

- `TenantId`
- `Name`
- `NormalizedName`
- `Description`
- `IsActive`
- `IsSystem`
- navigation: `UserRoles`, `RolePermissions`

`Permission`

- `Code`
- `Name`
- `Resource`
- `Action`
- `Description`
- `ConditionsJson`
- `IsActive`
- navigation: `RolePermissions`

`UserRole`

- composite key: `UserId`, `RoleId`
- `AssignedAtUtc`
- `AssignedBy`

`RolePermission`

- composite key: `RoleId`, `PermissionId`
- `AssignedAtUtc`
- `AssignedBy`

`ClientApp`

- `TenantId`
- `ClientId`
- `Name`
- `SecretHash`
- `Type`
- `AllowedScopes`
- `AllowedOrigins`
- `IsActive`

`RefreshToken`

- `UserId`
- optional `ClientAppId`
- `TokenHash`
- `JwtId`
- `ExpiresAtUtc`
- `RevokedAtUtc`
- `RevocationReason`
- `ReplacedByTokenId`
- `DeviceName`
- `IpAddress`
- `UserAgent`

Derived behavior:

- `IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow`

`RevokedAccessToken`

- `JwtId`
- optional `UserId`
- `ExpiresAtUtc`
- `Reason`

`AuditLog`

- optional `TenantId`
- optional `UserId`
- optional `ClientAppId`
- `Action`
- `EntityType`
- optional `EntityId`
- optional `MetadataJson`
- optional `IpAddress`
- optional `UserAgent`
- optional `CorrelationId`
- `Result`

### 7.3 Enums

`UserStatus`

- `Pending`
- `Active`
- `Locked`
- `Disabled`

`ClientAppType`

- `Confidential`
- `Public`
- `Internal`

## 8. Persistence Model

EF Core uses SQL Server via `UseSqlServer(...)` and the connection string `ConnectionStrings:AuthDb`.

Configured tables:

- `tenants`
- `users`
- `roles`
- `permissions`
- `user_roles`
- `role_permissions`
- `client_apps`
- `refresh_tokens`
- `revoked_access_tokens`
- `audit_logs`

Important constraints and indexes:

- `users.NormalizedEmail` unique
- `roles(TenantId, NormalizedName)` unique
- `permissions.Code` unique
- `client_apps.ClientId` unique
- `refresh_tokens.TokenHash` unique
- `refresh_tokens(UserId, ExpiresAtUtc)` indexed
- `revoked_access_tokens.JwtId` unique
- `audit_logs.CreatedAtUtc` indexed
- `audit_logs(UserId, CreatedAtUtc)` indexed

Delete behavior highlights:

- `User -> Tenant`: restrict
- `Role -> Tenant`: restrict
- `RefreshToken -> ClientApp`: set null on delete

## 9. Repository Behavior

### 9.1 User repository

`UserRepository` supports:

- add user
- load user by id
- load user by normalized email
- check email uniqueness
- list active role names for a user
- list active permission codes derived from active roles and active permissions
- assign roles by replacing existing user-role rows with the provided set

### 9.2 Role repository

`RoleRepository` supports:

- add/remove role
- load role
- uniqueness by normalized name and tenant
- list roles by tenant filter
- assign permissions by replacing existing role-permission rows with the provided set

### 9.3 Permission repository

`PermissionRepository` supports:

- add/remove permission
- get by id
- get by ids
- check uniqueness by code
- list all permissions ordered by code

### 9.4 Token repositories

`RefreshTokenRepository`

- add refresh token
- get refresh token by token hash

`RevokedAccessTokenRepository`

- add revoked access token
- check whether a revoked token with the same `jti` is still active

### 9.5 Audit repository

`AuditLogRepository`

- append-only add operation

## 10. Application Layer Use Cases

### 10.1 Auth

`RegisterUserCommand`

- validates email, username, password
- checks normalized email uniqueness
- creates `User`
- hashes password through `IPasswordService`
- saves user
- writes audit log `user.registered`
- returns `RegisteredUserResponse`

`LoginCommand`

- validates input
- loads user by normalized email
- verifies password
- optionally validates `ClientApp`
- calculates current roles and permissions
- generates JWT + refresh token
- stores hashed refresh token
- updates `LastLoginAtUtc`
- writes audit `auth.login`
- returns `TokenResponse`

`RefreshTokenCommand`

- hashes incoming token
- loads refresh token row and user
- rejects inactive token
- optionally validates `ClientApp`
- recalculates roles and permissions
- generates new tokens
- stores next refresh token
- revokes current refresh token with replacement reference
- writes audit `auth.token.refreshed`

`ChangePasswordCommand`

- requires authenticated user from `ICurrentUserContext`
- verifies current password
- updates password hash
- saves changes
- writes audit `auth.password.changed`

`RevokeTokenCommand`

- requires either refresh token or access token
- revokes one or both independently
- writes audit `auth.token.revoked`

### 10.2 Roles

`CreateRoleCommand`

- checks uniqueness on normalized role name within tenant
- creates and saves role
- writes audit `role.created`

`UpdateRoleCommand`

- loads role
- updates name, description, active flag
- saves and audits `role.updated`

`DeleteRoleCommand`

- loads role
- removes role
- saves and audits `role.deleted`

`AssignPermissionsToRoleCommand`

- loads role
- loads all requested permissions
- fails if any permission id is missing
- replaces role-permission associations
- saves and audits `role.permissions.assigned`

`ListRolesQuery`

- returns role DTOs with optional tenant filter

### 10.3 Permissions

`CreatePermissionCommand`

- checks uniqueness by code
- creates permission
- saves and audits `permission.created`

`UpdatePermissionCommand`

- updates mutable fields and `IsActive`
- saves and audits `permission.updated`

`DeletePermissionCommand`

- removes permission
- saves and audits `permission.deleted`

`ListPermissionsQuery`

- returns all permission DTOs

### 10.4 Users

`AssignRolesToUserCommand`

- loads user
- loads all requested roles
- fails if any role id is missing
- replaces user-role associations
- saves and audits `user.roles.assigned`

`CheckUserPermissionQuery`

- loads user
- calculates effective permissions
- returns whether a permission code is granted, case-insensitive

## 11. Validation, Logging and Error Handling

### 11.1 Validation

FluentValidation validators are defined next to commands/queries.

`ValidationBehavior<TRequest, TResponse>`:

- runs all validators for the request
- aggregates validation failures
- throws `ValidationException` if any rule fails

### 11.2 Logging

`LoggingBehavior<TRequest, TResponse>` logs:

- request start with request type and payload
- request completion with request type

Additionally, Serilog request logging is enabled globally in the HTTP pipeline.

### 11.3 Error handling

`GlobalExceptionHandler` maps:

- `ValidationException` -> `400 Validation failed`
- `AppException` -> status code defined by the exception
- any other exception -> `500 An unexpected error occurred`

Error format is ASP.NET Core `ProblemDetails`, including:

- `status`
- `title`
- `detail`
- `instance`
- `traceId`
- optional `errors` dictionary for validation failures

## 12. Current User Context

`HttpCurrentUserContext` extracts request-scoped data from `HttpContext` and JWT claims:

- `UserId`
- `TenantId`
- `Email`
- `ClientId`
- `IpAddress`
- `UserAgent`
- `CorrelationId`
- `IsAuthenticated`

This service is used by handlers and `AuditService`.

## 13. Seeding and Default Data

During startup, `InitializeDatabaseAsync()` calls:

1. `Database.MigrateAsync()`
2. `DbSeeder.SeedAsync()`

Seeding behavior:

- skips if any user already exists
- creates one default tenant
- creates all built-in permissions from `PermissionCodes.All`
- creates roles:
  - `Administrator`
  - `User`
- grants all built-in permissions to `Administrator`
- creates one admin user
- assigns the admin user to `Administrator`
- creates one default client app:
  - `default-web`

Default seed values come from configuration:

- admin email: `admin@local.dev`
- admin username: `admin`
- admin password: `Admin@123456`
- tenant name: `Default Tenant`
- tenant slug: `default`

## 14. Configuration

Primary settings are in `Auth.Api/appsettings.json` and `Auth.Api/appsettings.Development.json`.

### 14.1 Connection string

Key:

- `ConnectionStrings:AuthDb`

The code expects SQL Server.

### 14.2 JWT settings

Section:

- `Jwt`

Fields:

- `Issuer`
- `Audience`
- `SigningKey`
- `AccessTokenMinutes`
- `RefreshTokenDays`

### 14.3 Seed settings

Section:

- `Seed`

Fields:

- `AdminEmail`
- `AdminUserName`
- `AdminPassword`
- `DefaultTenantName`
- `DefaultTenantSlug`

### 14.4 Logging

Serilog is configured from the `Serilog` section and currently writes to console.

## 15. Rate Limiting

Two fixed-window limiters are configured:

- `auth`
  - 10 requests per minute
  - used by register, login, refresh token

- `default`
  - 60 requests per minute
  - used by authenticated business endpoints

Rejected requests return HTTP `429`.

## 16. Swagger and API Exploration

Swagger is enabled unconditionally in `Program.cs`.

OpenAPI configuration includes:

- document name: `v1`
- title: `Auth Service API`
- bearer security definition
- global bearer security requirement

Sample requests are also available in `Auth.Api/Auth.Api.http`.

## 17. Deployment and Local Run

### 17.1 Local run

Typical command:

```bash
dotnet run --project Auth.Api/Auth.Api.csproj
```

Default API base URL in sample HTTP file:

- `http://localhost:8080`

### 17.2 Docker Compose

`docker-compose.yml` defines:

- `sqlserver`
  - SQL Server 2022 container
  - exposed on host port `1433`

- `auth-api`
  - builds from `Auth.Api/Dockerfile`
  - exposed on host port `8080`
  - injects connection string, JWT values, and seed config through environment variables

### 17.3 Dockerfile

The Dockerfile:

- builds with `.NET SDK 10.0`
- publishes `Auth.Api`
- runs on `.NET ASP.NET Runtime 8.0`
- binds to `http://+:8080`

## 18. Notable Design Decisions

- Minimal API instead of MVC controllers
- MediatR for use-case dispatching
- FluentValidation for request validation
- EF Core repositories over direct DbContext usage in handlers
- JWT access token kept stateless, with revocation implemented through denylist by `jti`
- Refresh token persisted as hash instead of plaintext
- Permission claims embedded in token for fast downstream authorization
- Audit logging performed from application handlers

## 19. Important Operational Notes

- The API applies a fallback authorization policy, so new endpoints are authenticated by default unless explicitly marked anonymous.
- Role and permission assignment methods are replace-style operations, not additive operations.
- Access token revocation only becomes effective because token validation checks `revoked_access_tokens`.
- Audit writes call `SaveChangesAsync()` internally, so handlers that also save state will usually persist business data first, then persist the audit row in a second transaction.
- Seed data only runs when there are no users in the database.
- Existing access tokens keep their embedded permissions until they expire or are refreshed.

## 20. Known Gaps / Future Work Visible from the Code

The following concepts exist structurally but are not yet fully exposed by API endpoints:

- tenant administration
- client app administration
- richer client secret validation or OAuth flows
- email confirmation flow
- user lock/disable management endpoints
- domain event dispatching from `UserRegisteredDomainEvent`
- use of `ConditionsJson` for policy/ABAC evaluation
- `Auth.AppHost` orchestration beyond an empty Aspire shell
- `Auth.ServiceDefaults` integration into the running API

## 21. Quick Reference

### Default seed login

```json
{
  "email": "admin@local.dev",
  "password": "Admin@123456",
  "clientId": "default-web"
}
```

### Main successful response wrapper

```json
{
  "data": {}
}
```

### Main error response shape

```json
{
  "type": "about:blank",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation failures have occurred.",
  "instance": "/api/v1/auth/login",
  "traceId": "00-...",
  "errors": {
    "Email": [
      "Email is required."
    ]
  }
}
```
