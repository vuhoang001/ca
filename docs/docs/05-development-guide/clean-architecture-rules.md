---
title: Clean Architecture Rules
---

# Clean Architecture Rules

## Dependency direction

```
Domain ← Application ← Infrastructure
           ↑                 ↑
           API ──────────────┘
```

**Domain** knows nothing. **Application** knows Domain and Shared abstractions. **Infrastructure** implements everything. **API** wires it all together.

## File placement

- New code **must** go under `Src/` — each project compiles only `Src/**/*.cs` (and `Migrations/**/*.cs` for Infrastructure).
- Infrastructure configs go in `Infrastructure/Src/Configurations/`.
- Use `ApplyConfigurationsFromAssembly` (already wired) — no manual registration.

## Naming conventions

| Thing | Convention | Example |
|---|---|---|
| Commands | `{Verb}{Entity}Command` | `CreateProductCommand` |
| Queries | `{Get/List}{Entity}Query` | `ListProductsQuery` |
| Handlers | `{CommandOrQuery}Handler` | `CreateProductCommandHandler` |
| Validators | `{Command}Validator` | `CreateProductCommandValidator` |
| Repos (interface) | `I{Entity}Repository` | `IProductRepository` |
| Repos (impl) | `{Entity}Repository` | `ProductRepository` |

## Shared library rules

`Shared.csproj` is a dependency of every layer — keep it minimal:
- Abstractions: `ICurrentUserContext`, `IUnitOfWork`, `IDateTimeProvider`, `IDomainEventDispatcher`
- Exceptions: `AppException` and its subtypes (`NotFoundException`, `ConflictException`, etc.)
- Primitives: `Entity`, `AuditableEntity`, `DomainEvent`
- Results: `ApiEnvelope<T>`

Do **not** add infrastructure concerns (EF Core, HTTP, Keycloak) to Shared.
