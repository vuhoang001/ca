using Shared;

namespace Domain.Entities;

public sealed class Role : AuditableEntity
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RolePermission> _rolePermissions = [];

    public Guid? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public string Name { get; private set; } = null!;
    public string NormalizedName { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public bool IsSystem { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    private Role()
    {
    }

    public Role(Guid? tenantId, string name, string? description, bool isSystem = false)
    {
        TenantId = tenantId;
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
        IsSystem = isSystem;
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
        IsActive = isActive;
    }
}