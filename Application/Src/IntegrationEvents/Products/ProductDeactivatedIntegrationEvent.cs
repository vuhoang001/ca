using Shared.Messaging;

namespace Application.IntegrationEvents.Products;

public sealed class ProductDeactivatedIntegrationEvent : IntegrationEvent
{
    public required Guid ProductId { get; init; }
    public Guid? TenantId { get; init; }
}
