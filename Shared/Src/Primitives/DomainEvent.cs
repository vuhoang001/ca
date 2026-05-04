using MediatR;

namespace Shared.Primitives;

public abstract class DomainEvent : INotification
{
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;
}