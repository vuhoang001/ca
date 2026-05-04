using Api.Application;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class PermissionRepository(AppDbContext dbContext) : IPermissionRepository
{
    public void Add(Permission permission) => dbContext.Permissions.Add(permission);

    public void Remove(Permission permission) => dbContext.Permissions.Remove(permission);

    public Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
    {
        return dbContext.Permissions.FirstOrDefaultAsync(x => x.Id == permissionId, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return dbContext.Permissions.AnyAsync(x => x.Code == code, cancellationToken);
    }

    public Task<List<Permission>> GetByIdsAsync(IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Permissions.Where(x => permissionIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task<List<Permission>> ListAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Permissions.OrderBy(x => x.Code).ToListAsync(cancellationToken);
    }
}