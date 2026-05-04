using System.Collections.Immutable;
using Shared.Primitives;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Auth.Shared.Extensions.EF;


public class EventDispatchInterceptor(
    IDomainEventDispatcher dispatcher,
    ILogger<EventDispatchInterceptor> logger) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        var ctx = eventData.Context;
        if (ctx is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entitiesWithEvents = ctx
            .ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(x => x.DomainEvents.Count != 0)
            .ToImmutableList();

        logger.LogInformation(
            "EventDispatchInterceptor: SavingChanges starting, found {EntityCount} entities with domain events. Dispatching now.",
            entitiesWithEvents.Count
        );

        await dispatcher.DispatchAndClearEvents(entitiesWithEvents);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
