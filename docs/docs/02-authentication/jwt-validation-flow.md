---
title: JWT Validation Flow
---

# JWT Validation Flow

## How it works

```
1. Client obtains JWT from Keycloak (token endpoint)
2. Client sends: Authorization: Bearer <jwt>
3. JwtBearerHandler intercepts the request
4. Handler fetches JWKS from {Authority}/.well-known/openid-configuration
5. Handler verifies RS256 signature using the public key from JWKS
6. Handler validates: iss, aud, exp, nbf
7. ClaimsPrincipal is populated from token claims
8. KeycloakClaimsTransformation runs → maps realm_access.roles to ClaimTypes.Role
9. Authorization policies check ClaimsPrincipal.IsInRole(...)
```

## Configuration

```json title="appsettings.json"
{
  "Keycloak": {
    "Authority": "http://localhost:8181/realms/masterdata",
    "Audience": "masterdata-api",
    "ClientId": "masterdata-api",
    "RequireHttpsMetadata": false
  }
}
```

In production, set `RequireHttpsMetadata: true` and use an HTTPS Keycloak URL.

## Symmetric vs. Asymmetric

The old auth system used HS256 (symmetric — shared secret). Keycloak uses **RS256** (asymmetric):

| | HS256 (old) | RS256 (Keycloak) |
|---|---|---|
| Signing | HMAC shared secret | RSA private key |
| Verification | Same secret | RSA public key (from JWKS) |
| Secret sharing | Required for all services | Not required |
| Key rotation | Manual update everywhere | Automatic via JWKS |

## Claims transformation

`KeycloakClaimsTransformation` (registered as `IClaimsTransformation`) runs after JWT validation and adds Keycloak roles to `ClaimTypes.Role`:

```csharp
// realm_access.roles → ClaimTypes.Role
// resource_access.{clientId}.roles → ClaimTypes.Role
```

This makes `[Authorize(Roles = "admin")]` and `RequireRole("admin")` work transparently.
