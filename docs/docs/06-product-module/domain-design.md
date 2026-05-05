---
title: Product Domain Design
---

# Product Domain Design

## Entity

```csharp
public sealed class Product : AuditableEntity
{
    public string Sku { get; private set; }      // normalized: UPPER-CASE, trimmed
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; }  // ISO 4217, e.g. "USD"
    public bool IsActive { get; private set; }
    public Guid? TenantId { get; private set; }   // null = global product
}
```

## Rules

- `Sku` is normalized to upper-case on creation. Unique per `(TenantId, Sku)`.
- `Currency` is normalized to upper-case (ISO 4217: USD, EUR, VND).
- `Price` must be ≥ 0.
- Creating a product with a duplicate SKU throws `ConflictException` (→ HTTP 409).
- Deleting a product is a **physical delete** (no soft-delete in this template). Add `IsActive = false` + filter if you need soft-delete.

## Inherited audit fields (from `AuditableEntity`)

| Field | Set when |
|---|---|
| `CreatedAtUtc` | INSERT |
| `CreatedBy` | INSERT — from `ICurrentUserContext.Email` |
| `LastModifiedAtUtc` | INSERT + UPDATE |
| `LastModifiedBy` | INSERT + UPDATE — from `ICurrentUserContext.Email` |

## Database schema

```sql
CREATE TABLE products (
    Id             UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    Sku            NVARCHAR(100) NOT NULL,
    Name           NVARCHAR(300) NOT NULL,
    Description    NVARCHAR(2000) NULL,
    Price          DECIMAL(18,4) NOT NULL,
    Currency       NVARCHAR(3) NOT NULL,
    IsActive       BIT NOT NULL,
    TenantId       UNIQUEIDENTIFIER NULL,
    CreatedAtUtc   DATETIME2 NOT NULL,
    CreatedBy      NVARCHAR(MAX) NULL,
    LastModifiedAtUtc DATETIME2 NULL,
    LastModifiedBy NVARCHAR(MAX) NULL,
    UNIQUE (Sku),
    UNIQUE (TenantId, Sku)
);
```
