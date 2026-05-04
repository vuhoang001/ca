using MediatR;

namespace Auth.Shared.Primitives;

public interface IHasDomainEvents : INotification
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
}