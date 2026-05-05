using Domain.Events;
using Shared;
using Shared.Kernel;

namespace Domain.Entities;

public sealed class Product : AuditableEntity
{
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public Guid? TenantId { get; private set; }

    private Product()
    {
    }

    public static Product Create(
        string sku,
        string name,
        string? description,
        decimal price,
        string currency,
        Guid? tenantId = null)
    {
        var product = new Product
        {
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Currency = currency.Trim().ToUpperInvariant(),
            IsActive = true,
            TenantId = tenantId
        };

        product.RegisterDomainEvent(new ProductCreatedDomainEvent(
            product.Id, product.Sku, product.Name, product.Description,
            product.Price, product.Currency, product.TenantId));

        return product;
    }

    public void Update(string name, string? description, decimal price, string currency)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        Currency = currency.Trim().ToUpperInvariant();

        RegisterDomainEvent(new ProductUpdatedDomainEvent(Id, Name, Description, Price, Currency));
    }

    public void Deactivate()
    {
        IsActive = false;
        RegisterDomainEvent(new ProductDeactivatedDomainEvent(Id, TenantId));
    }

    public void Activate()
    {
        IsActive = true;
        RegisterDomainEvent(new ProductActivatedDomainEvent(Id, TenantId));
    }
}