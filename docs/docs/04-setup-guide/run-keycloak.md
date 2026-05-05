---
title: Run Keycloak
---

# Running Keycloak Locally

## Option A — docker-compose (recommended)

```bash
docker compose up -d mssql keycloak
```

Keycloak imports the realm from `keycloak/realm-export.json` automatically. Wait ~60s for startup.

- **Admin UI:** http://localhost:8181 (admin / admin)
- **Realm:** http://localhost:8181/realms/masterdata

## Option B — .NET Aspire

```bash
dotnet run --project AppHost/AppHost.csproj
```

Opens the Aspire dashboard. Keycloak and SQL Server start automatically.

## Getting a token (curl)

```bash
curl -s -X POST \
  http://localhost:8181/realms/masterdata/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password" \
  -d "client_id=masterdata-api" \
  -d "client_secret=masterdata-api-secret-change-in-prod" \
  -d "username=admin" \
  -d "password=Admin@123456" \
  | jq .access_token
```

## Test users

| Username | Password | Roles |
|---|---|---|
| admin | Admin@123456 | admin, masterdata-writer, masterdata-reader |
| writer | Writer@123456 | masterdata-writer, masterdata-reader |
| reader | Reader@123456 | masterdata-reader |
