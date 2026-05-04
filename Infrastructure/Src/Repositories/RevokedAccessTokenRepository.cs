using Api.Application;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RevokedAccessTokenRepository(AppDbContext dbContext) : IRevokedAccessTokenRepository
{
    public void Add(RevokedAccessToken token) => dbContext.RevokedAccessTokens.Add(token);

    public Task<bool> ExistsActiveAsync(string jwtId, CancellationToken cancellationToken = default)
    {
        return dbContext.RevokedAccessTokens.AnyAsync(x => x.JwtId == jwtId && x.ExpiresAtUtc > DateTime.UtcNow,
                                                      cancellationToken);
    }
}