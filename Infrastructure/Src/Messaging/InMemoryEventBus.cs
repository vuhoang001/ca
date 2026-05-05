using Application.Abstractions;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Infrastructure.Messaging;

// Dùng cho unit test — không phụ thuộc vào MassTransit.
// Production dùng MassTransitEventBus (được đăng ký trong DependencyInjection.cs).
public sealed class InMemoryEventBus(ILogger<InMemoryEventBus> logger) : IEventBus
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IntegrationEvent
    {
        logger.LogInformation(
            "[EventBus:Mock] {EventType} | Id={EventId} | OccurredAt={OccurredAt} | CorrelationId={CorrelationId} | Payload={@Payload}",
            typeof(T).Name,
            integrationEvent.Id,
            integrationEvent.OccurredAtUtc,
            integrationEvent.CorrelationId,
            integrationEvent);

        return Task.CompletedTask;
    }
}
