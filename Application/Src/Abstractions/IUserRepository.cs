using Domain.Entities;

namespace Api.Application;

public interface IUserRepository
{
    void Add(User user);
    Task<User?> GetByIdAsync(Guid userId, bool includeRoles = false, CancellationToken cancellationToken = default);

    Task<User?> GetByNormalizedEmailAsync(string normalizedEmail, bool includeRoles = false,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<List<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetPermissionCodesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AssignRolesAsync(User user, IReadOnlyCollection<Role> roles, string? assignedBy,
        CancellationToken cancellationToken = default);
}