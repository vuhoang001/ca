---
title: Architecture Overview
---

# Architecture Overview

## Layer dependency graph

```
┌─────────────────────────────┐
│         API Layer           │  ← Minimal API endpoints, middleware, DI wiring
│  (Api.csproj)               │
└──────────┬──────────────────┘
           │ references
┌──────────▼──────────────────┐
│      Application Layer      │  ← MediatR handlers, validators, DTOs
│  (Application.csproj)       │
└──────────┬──────────────────┘
           │ references
┌──────────▼──────────────────┐
│       Domain Layer          │  ← Entities, domain logic — ZERO external deps
│  (Domain.csproj)            │
└─────────────────────────────┘

┌─────────────────────────────┐
│    Infrastructure Layer     │  ← EF Core, Keycloak JWT, repos, services
│  (Infrastructure.csproj)    │
│  references: Application +  │
│  Domain + Shared            │
└─────────────────────────────┘

┌─────────────────────────────┐
│       Shared Library        │  ← Interfaces, primitives, exceptions, envelopes
│  (Shared.csproj)            │
└─────────────────────────────┘
```

## Key rules

1. **Domain has zero external dependencies** — no NuGet packages, no framework references.
2. **Infrastructure is the only layer that knows about Keycloak, EF Core, SQL Server.**
3. **Application talks to Infrastructure only via interfaces** (`IProductRepository`, `ICurrentUserContext`, etc.)
4. **New code goes under `Src/`** — each project has `EnableDefaultCompileItems=false`, so files outside `Src/` (and `Migrations/`) are silently excluded.

## Request flow

```
HTTP Request
    │
    ▼
GlobalExceptionHandler (middleware)
    │
    ▼
JwtBearerHandler → KeycloakClaimsTransformation (adds roles to ClaimsPrincipal)
    │
    ▼
ProductEndpoints (minimal API)
    │
    ▼
IMediator.Send(command/query)
    │
    ▼
ValidationBehavior → validates via FluentValidation
    │
    ▼
LoggingBehavior → structured log entry
    │
    ▼
CommandHandler / QueryHandler
    │
    ▼
IProductRepository → AppDbContext → SQL Server
    │
    ▼
ApiEnvelope<T> → JSON response
```
