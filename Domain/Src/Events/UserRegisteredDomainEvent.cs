using Shared.Primitives;

namespace Domain.Events;

public sealed class UserRegisteredDomainEvent(Guid userId, string email) : DomainEvent
{
    public Guid UserId { get; } = userId;
    public string Email { get; } = email;
}