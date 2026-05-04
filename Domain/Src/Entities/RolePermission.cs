namespace Domain.Entities;

public sealed class RolePermission
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;
    public DateTime AssignedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? AssignedBy { get; private set; }

    private RolePermission()
    {
    }

    public RolePermission(Guid roleId, Guid permissionId, string? assignedBy)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        AssignedBy = assignedBy;
    }
}
