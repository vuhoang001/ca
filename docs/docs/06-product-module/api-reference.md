---
title: API Reference
---

# Product API Reference

Base path: `/api/v1/products`

All endpoints require a valid Keycloak JWT in the `Authorization: Bearer <token>` header.

## List products

```
GET /api/v1/products
```

**Required role:** `masterdata-reader`, `masterdata-writer`, or `admin`

**Query parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `page` | int | 1 | Page number |
| `pageSize` | int | 20 | Items per page (max 100) |
| `search` | string | — | Filters Name and Sku (contains) |
| `isActive` | bool? | — | Filter by active status |
| `sortBy` | string | `name` | `name`, `sku`, `price`, `createdat` |
| `sortDescending` | bool | false | Sort direction |

**Response:**

```json
{
  "data": {
    "items": [...],
    "totalCount": 42,
    "page": 1,
    "pageSize": 20,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

## Get product by ID

```
GET /api/v1/products/{id}
```

Returns 404 if not found.

## Create product

```
POST /api/v1/products
```

**Required role:** `masterdata-writer` or `admin`

**Body:**

```json
{
  "sku": "PROD-001",
  "name": "Laptop Stand",
  "description": "Adjustable aluminum laptop stand",
  "price": 49.99,
  "currency": "USD",
  "tenantId": null
}
```

Returns 201 Created with `Location` header and the created product.

## Update product

```
PUT /api/v1/products/{id}
```

**Required role:** `masterdata-writer` or `admin`

**Body:**

```json
{
  "name": "Laptop Stand Pro",
  "description": "Updated description",
  "price": 59.99,
  "currency": "USD"
}
```

SKU cannot be changed after creation.

## Delete product

```
DELETE /api/v1/products/{id}
```

**Required role:** `masterdata-writer` or `admin`

Returns 204 No Content. Returns 404 if not found.
