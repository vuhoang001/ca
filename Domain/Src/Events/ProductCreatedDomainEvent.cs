using Shared.Primitives;

namespace Domain.Events;

public sealed class ProductCreatedDomainEvent(
    Guid productId,
    string sku,
    string name,
    string? description,
    decimal price,
    string currency,
    Guid? tenantId) : DomainEvent
{
    public Guid ProductId { get; } = productId;
    public string Sku { get; } = sku;
    public string Name { get; } = name;
    public string? Description { get; } = description;
    public decimal Price { get; } = price;
    public string Currency { get; } = currency;
    public Guid? TenantId { get; } = tenantId;
}