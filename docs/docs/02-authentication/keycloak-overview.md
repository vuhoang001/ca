---
title: Keycloak Overview
---

# Keycloak Overview

This template **does not issue tokens** — it only validates them. Keycloak is the sole identity provider.

## Why Keycloak?

- Open-source, battle-tested IdP (Red Hat / CNCF ecosystem)
- Implements OAuth 2.0 + OIDC standards — any OIDC-compatible client works
- Realm-per-tenant isolation for multi-tenant systems
- Rich admin UI + REST API for user/role management
- JWKS endpoint: services validate tokens locally without calling back to Keycloak

## Realm: `masterdata`

The included `keycloak/realm-export.json` creates a realm called `masterdata` with:

| Resource | Value |
|---|---|
| Realm | `masterdata` |
| Client ID | `masterdata-api` |
| Client secret | `masterdata-api-secret-change-in-prod` |
| Predefined roles | `admin`, `masterdata-writer`, `masterdata-reader` |
| Test users | `admin` / `writer` / `reader` (see passwords in realm export) |

## Token endpoint (dev)

```
POST http://localhost:8181/realms/masterdata/protocol/openid-connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&client_id=masterdata-api
&client_secret=masterdata-api-secret-change-in-prod
&username=admin
&password=Admin@123456
```

## JWKS endpoint

```
GET http://localhost:8181/realms/masterdata/protocol/openid-connect/certs
```

The API fetches and caches this automatically on startup. No `SigningKey` config needed.
