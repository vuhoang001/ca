---
title: Docker Setup
---

# Docker Setup

## Development

```bash
# Start all services
docker compose up --build

# Start only dependencies (for local dotnet run)
docker compose up -d mssql keycloak

# View logs
docker compose logs -f api
docker compose logs -f keycloak

# Tear down (preserves volumes)
docker compose down

# Full reset including volumes
docker compose down -v
```

## Services

| Service | Port | Purpose |
|---|---|---|
| `mssql` | 1433 | SQL Server database |
| `keycloak` | 8181 | Keycloak identity provider |
| `api` | 8080 | MasterData API |

## Production (docker-compose.prod.yaml)

```bash
# Create .env file with secrets
cp .env.example .env
# Edit .env with real values

docker compose -f docker-compose.prod.yaml up -d
```

## Dockerfile

The API uses a multi-stage Dockerfile at `Api/Dockerfile`. Build the image:

```bash
docker build -f Api/Dockerfile -t masterdata-api:latest .
```
