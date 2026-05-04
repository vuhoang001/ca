using Shared.Primitives;

namespace Auth.Shared.Extensions.Repository;

public interface IRepository<T> where T : IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}