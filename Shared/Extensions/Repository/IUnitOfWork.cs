namespace Auth.Shared.Extensions.Repository;

public interface IUnitOfWork : IDisposable
{
   Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}