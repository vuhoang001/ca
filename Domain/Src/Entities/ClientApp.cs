using Domain.Enums;
using Shared;

namespace Domain.Entities;

public sealed class ClientApp : AuditableEntity
{
    public Guid? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }
    public string ClientId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? SecretHash { get; private set; }
    public ClientAppType Type { get; private set; }
    public string AllowedScopes { get; private set; } = string.Empty;
    public string AllowedOrigins { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private ClientApp()
    {
    }

    public ClientApp(Guid? tenantId, string clientId, string name, ClientAppType type, string? secretHash = null)
    {
        TenantId = tenantId;
        ClientId = clientId.Trim().ToLowerInvariant();
        Name = name.Trim();
        Type = type;
        SecretHash = secretHash;
    }
}
