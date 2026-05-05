---
title: Token Structure
---

# Token Structure

## Keycloak JWT payload (decoded)

```json
{
  "exp": 1746000000,
  "iat": 1745999100,
  "jti": "abc123...",
  "iss": "http://localhost:8181/realms/masterdata",
  "aud": ["masterdata-api", "account"],
  "sub": "00000000-0000-0000-0000-000000000001",
  "typ": "Bearer",
  "azp": "masterdata-api",
  "preferred_username": "admin",
  "email": "admin@masterdata.local",
  "email_verified": true,
  "realm_access": {
    "roles": ["admin", "masterdata-writer", "masterdata-reader"]
  },
  "resource_access": {
    "masterdata-api": {
      "roles": ["masterdata-writer"]
    }
  }
}
```

## Claims used by this service

| Claim | Used for |
|---|---|
| `sub` | `ICurrentUserContext.UserId` (Guid) |
| `preferred_username` | `ICurrentUserContext.Username` |
| `email` | `ICurrentUserContext.Email`, audit `CreatedBy` field |
| `realm_access.roles` | Authorization via `RequireRole(...)` |
| `resource_access.{clientId}.roles` | Client-level role authorization |
| `tenant_id` *(custom)* | `ICurrentUserContext.TenantId` — add via Keycloak mapper |

## Adding a custom `tenant_id` claim

In Keycloak Admin UI → Client → Mappers → Add mapper:
- **Type:** User Attribute
- **User Attribute:** `tenant_id`
- **Token Claim Name:** `tenant_id`
- **Claim JSON Type:** String

Then set `tenant_id` attribute on each user.
