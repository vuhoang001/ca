---
title: Add a New Module
---

# Adding a New Module

Follow these steps to add, for example, a `Category` module:

## 1. Domain entity

```csharp title="Domain/Src/Entities/Category.cs"
using Shared;

namespace Domain.Entities;

public sealed class Category : AuditableEntity
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private Category() { }

    public static Category Create(string code, string name) =>
        new() { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(), IsActive = true };
}
```

## 2. Repository interface

```csharp title="Application/Src/Abstractions/ICategoryRepository.cs"
using Domain.Entities;

namespace Application.Abstractions;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Remove(Category category);
}
```

## 3. Application features

Create `Application/Src/Features/Categories/Commands/CreateCategoryCommand.cs`, etc. — same pattern as Products.

## 4. EF Core configuration

```csharp title="Infrastructure/Src/Configurations/ModelConfigurations.cs"
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}
```

## 5. Repository implementation

```csharp title="Infrastructure/Src/Repositories/CategoryRepository.cs"
public sealed class CategoryRepository(AppDbContext dbContext) : ICategoryRepository { ... }
```

## 6. Register in DI

```csharp title="Infrastructure/Src/DependencyInjection.cs"
services.AddScoped<ICategoryRepository, CategoryRepository>();
```

## 7. Add DbSet to AppDbContext

```csharp
public DbSet<Category> Categories => Set<Category>();
```

## 8. Create endpoint module

```csharp title="Api/Src/Endpoints/V1/CategoryEndpoints.cs"
public sealed class CategoryEndpoints : IEndpointModule { ... }
```

## 9. Register endpoint

```csharp title="Api/Src/Endpoints/EndpointExtensions.cs"
IEndpointModule[] modules = [
    new V1.ProductEndpoints(),
    new V1.CategoryEndpoints(),   // add here
    new V1.HealthEndpoints()
];
```

## 10. Add migration

```bash
dotnet ef migrations add AddCategories \
  --project Infrastructure/Infrastructure.csproj \
  --startup-project Api/Api.csproj
```
