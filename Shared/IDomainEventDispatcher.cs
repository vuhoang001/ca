using System.Collections.Immutable;
using Shared.Primitives;

namespace Shared;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(ImmutableList<IHasDomainEvents> entitiesWithEvents);
}