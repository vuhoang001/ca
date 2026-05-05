---
title: Configure Backend
---

# Configuring the Backend

## appsettings.json

```json
{
  "ConnectionStrings": {
    "MasterDataDb": "Server=localhost,1433;Database=MasterDataDb;..."
  },
  "Keycloak": {
    "Authority": "http://localhost:8181/realms/masterdata",
    "Audience": "masterdata-api",
    "ClientId": "masterdata-api",
    "RequireHttpsMetadata": false
  }
}
```

## Environment variable overrides (docker-compose / k8s)

```yaml
environment:
  ConnectionStrings__MasterDataDb: "Server=mssql,1433;Database=MasterDataDb;..."
  Keycloak__Authority: "http://keycloak:8080/realms/masterdata"
  Keycloak__RequireHttpsMetadata: "false"
```

## Production config

```yaml
environment:
  Keycloak__Authority: "https://keycloak.yourdomain.com/realms/masterdata"
  Keycloak__RequireHttpsMetadata: "true"
  Keycloak__ClientId: "masterdata-api"
```

Set `RequireHttpsMetadata: true` and use a real TLS certificate in production.

## Database migrations

Migrations run automatically on startup (`MigrateAsync()` in `OpenApiExtensions.InitializeDatabaseAsync`).

To run manually:

```bash
dotnet ef database update \
  --project Infrastructure/Infrastructure.csproj \
  --startup-project Api/Api.csproj
```
