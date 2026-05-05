using Application.Abstractions;
using Application.IntegrationEvents.Products;
using Domain.Events;
using MediatR;
using Shared.Abstractions;

namespace Application.Features.Products.EventHandlers;

internal sealed class ProductActivatedDomainEventHandler(
    IEventBus eventBus,
    ICurrentUserContext currentUser) : INotificationHandler<ProductActivatedDomainEvent>
{
    public Task Handle(ProductActivatedDomainEvent notification, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(new ProductActivatedIntegrationEvent
        {
            ProductId = notification.ProductId,
            TenantId = notification.TenantId,
            CorrelationId = currentUser.CorrelationId
        }, cancellationToken);
}
