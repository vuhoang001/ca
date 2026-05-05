---
title: Protecting Endpoints
---

# Protecting Endpoints

## Basic pattern

```csharp
group.MapGet("/products", handler)
    .RequireAuthorization(p => p.RequireRole("masterdata-reader", "masterdata-writer", "admin"))
    .RequireRateLimiting("default");
```

## Allow anonymous

```csharp
app.MapGet("/public-info", handler)
    .AllowAnonymous();
```

## Custom policy

```csharp
// In DI setup
services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", p => p.RequireRole("admin"));

// In endpoint
.RequireAuthorization("AdminOnly")
```

## Rate limiting

A single rate limiter `"default"` (60 req/min per IP) is registered. Apply it on all business endpoints:

```csharp
.RequireRateLimiting("default")
```

Add additional named limiters in `Api/Program.cs` if you need stricter limits on specific routes.
