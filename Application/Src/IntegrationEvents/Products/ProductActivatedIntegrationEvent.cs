using Shared.Messaging;

namespace Application.IntegrationEvents.Products;

public sealed class ProductActivatedIntegrationEvent : IntegrationEvent
{
    public required Guid ProductId { get; init; }
    public Guid? TenantId { get; init; }
}
