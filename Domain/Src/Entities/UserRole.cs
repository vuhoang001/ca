namespace Domain.Entities;

public sealed class UserRole
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;
    public DateTime AssignedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? AssignedBy { get; private set; }

    private UserRole()
    {
    }

    public UserRole(Guid userId, Guid roleId, string? assignedBy)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
    }
}