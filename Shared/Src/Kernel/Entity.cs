using Shared.Primitives;

namespace Shared;

public abstract class Entity : HasDomainEvents
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
