---
title: Keycloak Roles Mapping
---

# Keycloak Roles Mapping

## How roles reach ASP.NET Core

```
Keycloak JWT
  realm_access.roles: ["admin", "masterdata-writer"]
        │
        ▼
KeycloakClaimsTransformation.TransformAsync()
        │
        ▼
ClaimsPrincipal gets new ClaimsIdentity with:
  ClaimTypes.Role = "admin"
  ClaimTypes.Role = "masterdata-writer"
        │
        ▼
endpoint: .RequireRole("masterdata-writer")  → ✅ passes
```

## Realm roles vs. Client roles

| Source | Claim path | When to use |
|---|---|---|
| Realm roles | `realm_access.roles` | Global roles across all clients |
| Client roles | `resource_access.{clientId}.roles` | Roles scoped to this specific API |

Both are parsed by `KeycloakClaimsTransformation`. Configure `Keycloak:ClientId` to enable client role parsing.

## Assigning roles via Keycloak Admin

1. Admin UI → Users → select user → Role Mappings
2. Select realm roles or client roles to assign
3. Save — next token issued for that user will include the new roles
