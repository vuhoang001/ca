using Shared.Primitives;

namespace Auth.Shared.Extensions.EventBus.Dispatcher;

public interface IEventMapper
{
    IntegrationEvent MapToIntegrationEvent(DomainEvent domainEvent);
}
