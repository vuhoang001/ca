using Shared;

namespace Domain.Entities;

public sealed class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public Guid? ClientAppId { get; private set; }
    public ClientApp? ClientApp { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public string JwtId { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevocationReason { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;

    private RefreshToken()
    {
    }

    public RefreshToken(
        Guid userId,
        Guid? clientAppId,
        string tokenHash,
        string jwtId,
        DateTime expiresAtUtc,
        string? deviceName,
        string? ipAddress,
        string? userAgent)
    {
        UserId = userId;
        ClientAppId = clientAppId;
        TokenHash = tokenHash;
        JwtId = jwtId;
        ExpiresAtUtc = expiresAtUtc;
        DeviceName = deviceName;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public void Revoke(string reason, Guid? replacedByTokenId = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        RevocationReason = reason;
        ReplacedByTokenId = replacedByTokenId;
    }
}