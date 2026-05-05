---
id: index
title: Introduction
sidebar_position: 1
slug: /
---

# MasterData Template

A production-ready **.NET 10 Clean Architecture** starter template for masterdata microservices.

## What is this template?

This template provides a fully wired, opinionated foundation for building masterdata services — services that manage reference data (products, categories, suppliers, etc.) consumed by other services.

**Key decisions baked in:**

| Concern | Choice |
|---|---|
| Architecture | Clean Architecture (Domain → Application → Infrastructure → API) |
| Authentication | Keycloak (OIDC/JWT) — no internal auth |
| Authorization | Keycloak realm roles via `[RequireRole]` |
| CQRS | MediatR with pipeline behaviors |
| Validation | FluentValidation (auto-wired via pipeline) |
| Persistence | EF Core 8 + SQL Server |
| Observability | Serilog + OpenTelemetry (via .NET Aspire ServiceDefaults) |
| Dev orchestration | .NET Aspire AppHost OR docker-compose |

## What's included

- **Product module** — full CRUD with pagination, filtering, and sorting
- **Keycloak integration** — JWT validation via JWKS auto-discovery, role mapping
- **Audit logging** — every mutating operation is stamped with user + timestamp
- **Multi-tenant ready** — optional `TenantId` on all masterdata entities
- **Test suite** — Domain unit tests, Application unit tests (Moq), Integration tests (Testcontainers)
- **Docusaurus docs** — this site

## Who should use this

Teams building microservices on .NET that:
- Already run (or plan to run) Keycloak as their identity provider
- Want Clean Architecture without writing the boilerplate themselves
- Need a production-ready starting point, not a tutorial

## Getting started

```bash
# Clone and rename
git clone https://github.com/your-org/masterdata-template myservice
cd myservice

# Start dependencies (SQL Server + Keycloak)
docker compose up -d mssql keycloak

# Run the API
dotnet run --project Api/Api.csproj
```

Swagger is available at `http://localhost:5xxx/swagger`. Get a token from Keycloak first (see [Setup Guide](../04-setup-guide/run-keycloak)).
