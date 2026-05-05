using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public void Add(User user) => dbContext.Users.Add(user);

    public Task<User?> GetByIdAsync(Guid userId, bool includeRoles = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = dbContext.Users;
        if (includeRoles)
        {
            query = query
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role);
        }

        return query.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, bool includeRoles = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = dbContext.Users;
        if (includeRoles)
        {
            query = query
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role);
        }

        return query.FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AnyAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    }

    public async Task<List<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserRoles
            .Where(x => x.UserId == userId && x.Role.IsActive)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetPermissionCodesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.UserRoles
            .Where(x => x.UserId == userId && x.Role.IsActive)
            .SelectMany(x => x.Role.RolePermissions)
            .Where(x => x.Permission.IsActive)
            .Select(x => x.Permission.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task AssignRolesAsync(User user, IReadOnlyCollection<Role> roles, string? assignedBy,
        CancellationToken cancellationToken = default)
    {
        var existingRoleIds = await dbContext.UserRoles
            .Where(x => x.UserId == user.Id)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        var incomingRoleIds = roles.Select(x => x.Id).ToHashSet();

        var toDelete = dbContext.UserRoles.Where(x => x.UserId == user.Id && !incomingRoleIds.Contains(x.RoleId));
        dbContext.UserRoles.RemoveRange(toDelete);

        foreach (var role in roles.Where(role => !existingRoleIds.Contains(role.Id)))
        {
            dbContext.UserRoles.Add(new UserRole(user.Id, role.Id, assignedBy));
        }
    }
}