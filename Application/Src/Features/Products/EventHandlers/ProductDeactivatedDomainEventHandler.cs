using Application.Abstractions;
using Application.IntegrationEvents.Products;
using Domain.Events;
using MediatR;
using Shared.Abstractions;

namespace Application.Features.Products.EventHandlers;

internal sealed class ProductDeactivatedDomainEventHandler(
    IEventBus eventBus,
    ICurrentUserContext currentUser) : INotificationHandler<ProductDeactivatedDomainEvent>
{
    public Task Handle(ProductDeactivatedDomainEvent notification, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(new ProductDeactivatedIntegrationEvent
        {
            ProductId = notification.ProductId,
            TenantId = notification.TenantId,
            CorrelationId = currentUser.CorrelationId
        }, cancellationToken);
}
