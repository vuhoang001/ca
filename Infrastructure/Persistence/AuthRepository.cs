using Application.Abstractions.Persistence;
using Shared.Extensions.Repository;
using Microsoft.EntityFrameworkCore;
using AuthEntity = Auth.Domain.Entities.Auth;

namespace Auth.Infrastructure.Persistences;

public class AuthRepository(AppDbContext dbContext) : IAuthRepository
{
    public IUnitOfWork UnitOfWork => dbContext;

    public Task<AuthEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Auths.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task AddAsync(AuthEntity user, CancellationToken cancellationToken = default)
    {
        await dbContext.Auths.AddAsync(user, cancellationToken);
    }
}

