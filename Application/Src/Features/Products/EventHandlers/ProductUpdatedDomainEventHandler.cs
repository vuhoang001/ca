using Application.Abstractions;
using Application.IntegrationEvents.Products;
using Domain.Events;
using MediatR;
using Shared.Abstractions;

namespace Application.Features.Products.EventHandlers;

internal sealed class ProductUpdatedDomainEventHandler(
    IEventBus eventBus,
    ICurrentUserContext currentUser) : INotificationHandler<ProductUpdatedDomainEvent>
{
    public Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken) =>
        eventBus.PublishAsync(new ProductUpdatedIntegrationEvent
        {
            ProductId = notification.ProductId,
            Name = notification.Name,
            Description = notification.Description,
            Price = notification.Price,
            Currency = notification.Currency,
            CorrelationId = currentUser.CorrelationId
        }, cancellationToken);
}
