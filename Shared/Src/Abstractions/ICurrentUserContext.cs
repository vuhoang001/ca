namespace Shared.Abstractions;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    Guid? TenantId { get; }
    IReadOnlyList<string> Roles { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? CorrelationId { get; }
    bool IsAuthenticated { get; }
}
