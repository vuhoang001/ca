using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Abstractions;

namespace Infrastructure.Services;

public sealed class HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    private HttpContext? HttpContext => httpContextAccessor.HttpContext;
    private ClaimsPrincipal? User => HttpContext?.User;

    public Guid? UserId => TryGetGuid(ClaimTypes.NameIdentifier) ?? TryGetGuid("sub");
    public Guid? TenantId => TryGetGuid("tenant_id");
    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.FindFirstValue("email");
    public string? ClientId => User?.FindFirstValue("client_id");
    public string? JwtId => User?.FindFirstValue("jti");
    public DateTime? AccessTokenExpiresAt
    {
        get
        {
            var exp = User?.FindFirstValue("exp");
            return long.TryParse(exp, out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                : null;
        }
    }
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