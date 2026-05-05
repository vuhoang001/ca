using Shared.Primitives;

namespace Domain.Events;

public sealed class ProductActivatedDomainEvent(Guid productId, Guid? tenantId) : DomainEvent
{
    public Guid ProductId { get; } = productId;
    public Guid? TenantId { get; } = tenantId;
}
