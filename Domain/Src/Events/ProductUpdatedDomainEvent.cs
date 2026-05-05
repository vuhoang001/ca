using Shared.Primitives;

namespace Domain.Events;

public sealed class ProductUpdatedDomainEvent(
    Guid productId,
    string name,
    string? description,
    decimal price,
    string currency) : DomainEvent
{
    public Guid ProductId { get; } = productId;
    public string Name { get; } = name;
    public string? Description { get; } = description;
    public decimal Price { get; } = price;
    public string Currency { get; } = currency;
}