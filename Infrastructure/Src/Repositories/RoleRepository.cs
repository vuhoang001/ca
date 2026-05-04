using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RoleRepository(AppDbContext dbContext) : IRoleRepository
{
    public void Add(Role role) => dbContext.Roles.Add(role);

    public void Remove(Role role) => dbContext.Roles.Remove(role);

    public Task<Role?> GetByIdAsync(Guid roleId, bool includePermissions = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Role> query = dbContext.Roles;
        if (includePermissions)
        {
            query = query
                .Include(x => x.RolePermissions)
                .ThenInclude(x => x.Permission);
        }

        return query.FirstOrDefaultAsync(x => x.Id == roleId, cancellationToken);
    }

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Roles.AnyAsync(x => x.NormalizedName == normalizedName && x.TenantId == tenantId,
                                        cancellationToken);
    }

    public Task<List<Role>> GetByIdsAsync(IReadOnlyCollection<Guid> roleIds,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Roles.Where(x => roleIds.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task<List<Role>> ListAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        return dbContext.Roles
            .Where(x => tenantId == null || x.TenantId == tenantId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AssignPermissionsAsync(Role role, IReadOnlyCollection<Permission> permissions, string? assignedBy,
        CancellationToken cancellationToken = default)
    {
        var existingPermissionIds = await dbContext.RolePermissions
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionId)
            .ToListAsync(cancellationToken);

        var incomingPermissionIds = permissions.Select(x => x.Id).ToHashSet();

        var toDelete =
            dbContext.RolePermissions.Where(x => x.RoleId == role.Id &&
                                                !incomingPermissionIds.Contains(x.PermissionId));
        dbContext.RolePermissions.RemoveRange(toDelete);

        foreach (var permission in permissions.Where(permission => !existingPermissionIds.Contains(permission.Id)))
        {
            dbContext.RolePermissions.Add(new RolePermission(role.Id, permission.Id, assignedBy));
        }
    }
}