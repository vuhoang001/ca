using Domain.Entities;

namespace Application.Abstractions;

public interface IPermissionRepository
{
    void Add(Permission permission);
    void Remove(Permission permission);
    Task<Permission?> GetByIdAsync(Guid permissionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<List<Permission>> GetByIdsAsync(IReadOnlyCollection<Guid> permissionIds,
        CancellationToken cancellationToken = default);

    Task<List<Permission>> ListAsync(CancellationToken cancellationToken = default);
}