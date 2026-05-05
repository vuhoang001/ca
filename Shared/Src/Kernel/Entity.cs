using Shared.Primitives;

namespace Shared.Kernel;

public abstract class Entity : HasDomainEvents
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
}