using System.Collections.Immutable;
using Shared.Primitives;

namespace Shared.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(ImmutableList<IHasDomainEvents> entitiesWithEvents);
}