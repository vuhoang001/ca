using Shared;

namespace Domain.Entities;

public sealed class RevokedAccessToken : AuditableEntity
{
    public string JwtId { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public string Reason { get; private set; } = null!;

    private RevokedAccessToken()
    {
    }

    public RevokedAccessToken(string jwtId, Guid? userId, DateTime expiresAtUtc, string reason)
    {
        JwtId = jwtId;
        UserId = userId;
        ExpiresAtUtc = expiresAtUtc;
        Reason = reason;
    }
}
