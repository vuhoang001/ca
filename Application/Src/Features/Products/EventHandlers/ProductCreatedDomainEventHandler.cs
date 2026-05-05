using Application.Abstractions;
using Application.IntegrationEvents.Products;
using Domain.Events;
using MediatR;
using Shared.Abstractions;

namespace Application.Features.Products.EventHandlers;

internal sealed class ProductCreatedDomainEventHandler(
    IEventBus eventBus,
    ICurrentUserContext currentUser) : INotificationHandler<ProductCreatedDomainEvent>
{
    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(new ProductCreatedIntegrationEvent
        {
            ProductId = notification.ProductId,
            Sku = notification.Sku,
            Name = notification.Name,
            Description = notification.Description,
            Price = notification.Price,
            Currency = notification.Currency,
            TenantId = notification.TenantId,
            CorrelationId = currentUser.CorrelationId
        }, cancellationToken);
}
