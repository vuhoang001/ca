using Application.Abstractions;
using MassTransit;
using Shared.Messaging;

namespace Infrastructure.Messaging;

public sealed class MassTransitEventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IntegrationEvent =>
        publishEndpoint.Publish(integrationEvent, cancellationToken);
}
