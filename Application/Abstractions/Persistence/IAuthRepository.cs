using Domain.Entities;
using Shared.Extensions.Repository;

namespace Auth.Application.Abstractions.Persistence;

public interface IAuthRepository : IRepository<Domain.Entities.Auth>
{
    Task<Domain.Entities.Auth?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task                        AddAsync(Domain.Entities.Auth user, CancellationToken cancellationToken = default);
}