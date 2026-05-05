---
title: Example Requests
---

# Example Requests

## Step 1: Get a token

```bash
TOKEN=$(curl -s -X POST \
  http://localhost:8181/realms/masterdata/protocol/openid-connect/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=password&client_id=masterdata-api&client_secret=masterdata-api-secret-change-in-prod&username=admin&password=Admin@123456" \
  | jq -r .access_token)
```

## Create a product

```bash
curl -s -X POST http://localhost:5000/api/v1/products \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "sku": "LAPTOP-001",
    "name": "MacBook Pro 14",
    "description": "Apple M3 Pro chip",
    "price": 1999.00,
    "currency": "USD"
  }' | jq .
```

## List products with filtering

```bash
curl -s "http://localhost:5000/api/v1/products?search=laptop&isActive=true&sortBy=price&sortDescending=true" \
  -H "Authorization: Bearer $TOKEN" | jq .
```

## Update a product

```bash
curl -s -X PUT http://localhost:5000/api/v1/products/{id} \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "MacBook Pro 14 (Updated)",
    "price": 1899.00,
    "currency": "USD"
  }' | jq .
```

## Delete a product

```bash
curl -s -X DELETE http://localhost:5000/api/v1/products/{id} \
  -H "Authorization: Bearer $TOKEN" \
  -w "\nHTTP Status: %{http_code}\n"
```
