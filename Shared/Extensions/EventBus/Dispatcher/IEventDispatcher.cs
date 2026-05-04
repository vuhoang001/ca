using Shared.Primitives;

namespace Auth.Shared.Extensions.EventBus.Dispatcher;

public interface IEventDispatcher
{
    Task DispatchAsync(DomainEvent @event, CancellationToken cancellationToken = default);
}
