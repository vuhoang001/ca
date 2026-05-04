using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Abstractions;

public interface IRoleRepository
{
    void Add(Role role);
    void Remove(Role role);
    Task<Role?> GetByIdAsync(Guid roleId, bool includePermissions = false,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? tenantId,
        CancellationToken cancellationToken = default);

    Task<List<Role>> GetByIdsAsync(IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default);
    Task<List<Role>> ListAsync(Guid? tenantId, CancellationToken cancellationToken = default);

    Task AssignPermissionsAsync(Role role, IReadOnlyCollection<Permission> permissions, string? assignedBy,
        CancellationToken cancellationToken = default);
}