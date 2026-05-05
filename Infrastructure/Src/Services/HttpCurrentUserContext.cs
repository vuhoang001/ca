using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Abstractions;

namespace Infrastructure.Services;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;
    private ClaimsPrincipal? User => HttpContext?.User;

    public Guid? UserId => TryGetGuid(ClaimTypes.NameIdentifier) ?? TryGetGuid("sub");
    public string? Username => User?.FindFirstValue("preferred_username");
    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");
    public Guid? TenantId => TryGetGuid("tenant_id");

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)[];

    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();
    public string? UserAgent => HttpContext?.Request.Headers.UserAgent.ToString();
    public string? CorrelationId => HttpContext?.TraceIdentifier;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    private Guid? TryGetGuid(string claimType)
    {
        var value = User?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var result) ? result : null;
    }
}
