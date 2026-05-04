using Shared.Primitives;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Auth.Shared.Extensions.EventBus.Dispatcher;

public class EventDispatcher(
    IPublishEndpoint bus,
    IEventMapper eventMapper,
    ILogger<EventDispatcher> logger) : IEventDispatcher
{
    public async Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        logger.LogInformation(
            "EventDispatcher: Starting to dispatch domain event {DomainEventType}",
            @event.GetType().Name
        );

        var integrationEvent = eventMapper.MapToIntegrationEvent(@event) ??
            throw new InvalidOperationException($"No integration event mapping found for '{@event.GetType().Name}'.");

        logger.LogInformation(
            "EventDispatcher: Mapped to integration event {IntegrationEventType} with Id {EventId}",
            integrationEvent.GetType().Name,
            integrationEvent.Id
        );

        try
        {
            await bus.Publish(integrationEvent.GetType(), integrationEvent, cancellationToken);

            logger.LogInformation(
                "EventDispatcher: Successfully published {IntegrationEventType} (EventId: {EventId}) to outbox/queue",
                integrationEvent.GetType().Name,
                integrationEvent.Id
            );
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "EventDispatcher: FAILED to publish {IntegrationEventType} (EventId: {EventId})",
                integrationEvent.GetType().Name,
                integrationEvent.Id
            );
            throw;
        }
    }
}