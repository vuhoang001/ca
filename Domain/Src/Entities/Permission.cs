using Shared;

namespace Domain.Entities;

public sealed class Permission : AuditableEntity
{
    private readonly List<RolePermission> _rolePermissions = [];

    public Guid? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string Resource { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? ConditionsJson { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions;

    private Permission()
    {
    }

    public Permission(Guid? tenantId, string code, string name, string resource, string action, string? description)
    {
        TenantId = tenantId;
        Code = code.Trim().ToLowerInvariant();
        Name = name.Trim();
        Resource = resource.Trim().ToLowerInvariant();
        Action = action.Trim().ToLowerInvariant();
        Description = description?.Trim();
    }

    public void Update(string name, string resource, string action, string? description, bool isActive)
    {
        Name = name.Trim();
        Resource = resource.Trim().ToLowerInvariant();
        Action = action.Trim().ToLowerInvariant();
        Description = description?.Trim();
        IsActive = isActive;
    }
}