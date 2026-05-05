using System.Security.Claims;
using System.Text.Json;
using Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Infrastructure.Keycloak;

public sealed class KeycloakClaimsTransformation(IOptions<KeycloakOptions> options) : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = new ClaimsIdentity();

        AddRolesFromClaim(principal, "realm_access", null, identity);

        if (options.Value.ClientId is { Length: > 0 } clientId)
            AddRolesFromResourceAccess(principal, clientId, identity);

        principal.AddIdentity(identity);
        return Task.FromResult(principal);
    }

    private static void AddRolesFromClaim(ClaimsPrincipal principal, string claimType, string? clientId, ClaimsIdentity identity)
    {
        var claimValue = principal.FindFirst(claimType)?.Value;
        if (claimValue is null) return;

        try
        {
            using var doc = JsonDocument.Parse(claimValue);
            if (doc.RootElement.TryGetProperty("roles", out var rolesEl))
                foreach (var r in rolesEl.EnumerateArray())
                    if (r.GetString() is { } role)
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        catch (JsonException) { }
    }

    private static void AddRolesFromResourceAccess(ClaimsPrincipal principal, string clientId, ClaimsIdentity identity)
    {
        var resourceAccess = principal.FindFirst("resource_access")?.Value;
        if (resourceAccess is null) return;

        try
        {
            using var doc = JsonDocument.Parse(resourceAccess);
            if (doc.RootElement.TryGetProperty(clientId, out var clientEl) &&
                clientEl.TryGetProperty("roles", out var rolesEl))
                foreach (var r in rolesEl.EnumerateArray())
                    if (r.GetString() is { } role)
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }
        catch (JsonException) { }
    }
}
