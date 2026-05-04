namespace Shared.Abstractions;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    string? Email { get; }
    string? ClientId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
    string? CorrelationId { get; }
    bool IsAuthenticated { get; }
}
