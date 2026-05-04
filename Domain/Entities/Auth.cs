using Domain.Event;
using Shared.Primitives;

namespace Auth.Domain.Entities;

public class Auth : Entity, IAggregateRoot
{
    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string EmailConfirmed { get; private set; }

    public string SecurityStamp { get; private set; }

    public AuthStatus Status { get; private set; }


    public Auth(string email, string passwordHash, string emailConfirmed, string securityStamp)
    {
        Email          = email;
        PasswordHash   = passwordHash;
        EmailConfirmed = emailConfirmed;
        SecurityStamp  = securityStamp;
        Status         = AuthStatus.Active;

        RegisterDomainEvent(new UserRegisteredEvent(this));
    }
}