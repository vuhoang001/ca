using Domain.Enums;
using Domain.Events;
using Shared;

namespace Domain.Entities;

public sealed class User : AuditableEntity
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    public Guid? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public string UserName { get; private set; } = null!;
    public string NormalizedUserName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string NormalizedEmail { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString("N");
    public bool EmailConfirmed { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.Active;
    public DateTime? LastLoginAtUtc { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    private User()
    {
    }

    public User(Guid? tenantId, string userName, string email, string passwordHash, bool emailConfirmed = false)
    {
        TenantId = tenantId;
        UserName = userName.Trim();
        NormalizedUserName = userName.Trim().ToUpperInvariant();
        Email = email.Trim().ToLowerInvariant();
        NormalizedEmail = email.Trim().ToUpperInvariant();
        PasswordHash = passwordHash;
        EmailConfirmed = emailConfirmed;

        RegisterDomainEvent(new UserRegisteredDomainEvent(Id, Email));
    }

    public void SetPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid().ToString("N");
    }

    public void MarkLogin(DateTime atUtc)
    {
        LastLoginAtUtc = atUtc;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void Disable()
    {
        Status = UserStatus.Disabled;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }
}