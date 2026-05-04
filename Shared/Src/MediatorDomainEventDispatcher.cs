using System.Collections.Immutable;
using MediatR;
using Shared.Abstractions;
using Shared.Primitives;

namespace Shared;

public sealed class MediatorDomainEventDispatcher(IMediator publisher) : IDomainEventDispatcher
{
    public async Task DispatchAndClearEvents(ImmutableList<IHasDomainEvents> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
        {
            DomainEvent[] events = [.. entity.DomainEvents];
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await publisher.Publish(domainEvent);
            }
        }
    }
}