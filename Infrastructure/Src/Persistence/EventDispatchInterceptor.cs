using System.Collections.Immutable;
using Shared;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Abstractions;
using Shared.Primitives;

namespace Infrastructure.Persistence;

public sealed class EventDispatchInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entitiesWithEvents = dbContext.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count != 0)
            .ToImmutableList();

        if (entitiesWithEvents.Count != 0)
        {
            await dispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}