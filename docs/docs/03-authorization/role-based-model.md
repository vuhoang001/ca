---
title: Role-Based Model
---

# Role-Based Access Model

## Predefined roles

| Role | Description | Can do |
|---|---|---|
| `admin` | Full access | All operations |
| `masterdata-writer` | Write access | GET + POST + PUT + DELETE |
| `masterdata-reader` | Read-only | GET only |

## Fallback policy

All endpoints require an authenticated user by default (configured in `Infrastructure/Src/DependencyInjection.cs`):

```csharp
services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
```

To expose an endpoint anonymously, call `.AllowAnonymous()` explicitly.

## Role check locations

Roles are verified at the endpoint level:

```csharp
// Read operations
.RequireAuthorization(p => p.RequireRole("masterdata-reader", "masterdata-writer", "admin"))

// Write operations
.RequireAuthorization(p => p.RequireRole("masterdata-writer", "admin"))
```

The check uses `ClaimsPrincipal.IsInRole()`, which reads `ClaimTypes.Role` claims added by `KeycloakClaimsTransformation`.

## No database permission checks

There is **no permissions table** in this service. Roles come exclusively from the Keycloak JWT. Granting or revoking a role in Keycloak takes effect on the user's next token refresh.
