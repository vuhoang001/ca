using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ClientAppRepository(AppDbContext dbContext) : IClientAppRepository
{
    public Task<ClientApp?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        var normalizedClientId = clientId.Trim().ToLowerInvariant();
        return dbContext.ClientApps.FirstOrDefaultAsync(x => x.ClientId == normalizedClientId && x.IsActive, cancellationToken);
    }
}
