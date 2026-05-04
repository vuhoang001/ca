using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : IRefreshTokenRepository
{
    public void Add(RefreshToken refreshToken) => dbContext.RefreshTokens.Add(refreshToken);

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser = false, CancellationToken cancellationToken = default)
    {
        IQueryable<RefreshToken> query = dbContext.RefreshTokens;
        if (includeUser)
        {
            query = query.Include(x => x.User);
        }

        return query.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }
}
