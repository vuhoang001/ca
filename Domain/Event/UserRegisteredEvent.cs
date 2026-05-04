using Shared.Primitives;

namespace Auth.Domain.Event;

public class UserRegisteredEvent(Entities.Auth auth) : DomainEvent
{
    public Entities.Auth Auth { get; } = auth;
}