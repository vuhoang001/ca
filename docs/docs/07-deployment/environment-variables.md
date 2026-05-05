---
title: Environment Variables
---

# Environment Variables

All config keys follow ASP.NET Core convention: nested JSON keys map to `Parent__Child` env vars.

## Required

| Variable | Example | Description |
|---|---|---|
| `ConnectionStrings__MasterDataDb` | `Server=...;Database=MasterDataDb;...` | SQL Server connection string |
| `Keycloak__Authority` | `https://kc.example.com/realms/masterdata` | Keycloak realm URL |
| `Keycloak__Audience` | `masterdata-api` | Expected `aud` claim in JWT |

## Optional (with defaults)

| Variable | Default | Description |
|---|---|---|
| `Keycloak__ClientId` | `""` | Client ID for `resource_access` role parsing |
| `Keycloak__RequireHttpsMetadata` | `true` | Set `false` only for local dev with HTTP Keycloak |
| `ASPNETCORE_ENVIRONMENT` | `Production` | `Development` enables detailed errors |
| `ASPNETCORE_URLS` | `http://+:8080` | Listening URL |

## Serilog (logging)

Controlled via `Serilog` config section in `appsettings.json`. Override via:

```bash
Serilog__MinimumLevel__Default=Warning
```

## Production checklist

- [ ] `Keycloak__RequireHttpsMetadata` = `true`
- [ ] `Keycloak__Authority` points to HTTPS Keycloak URL
- [ ] `ConnectionStrings__MasterDataDb` uses a strong password
- [ ] `ASPNETCORE_ENVIRONMENT` = `Production`
- [ ] Secrets injected via secret manager or k8s secrets — not committed to repo
