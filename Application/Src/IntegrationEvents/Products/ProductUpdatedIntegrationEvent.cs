using Shared.Messaging;

namespace Application.IntegrationEvents.Products;

public sealed class ProductUpdatedIntegrationEvent : IntegrationEvent
{
    public required Guid ProductId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required decimal Price { get; init; }
    public required string Currency { get; init; }
}
