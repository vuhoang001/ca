using Shared;
using Shared.Kernel;

namespace Domain.Entities;

public sealed class AuditLog : AuditableEntity
{
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public string? EntityId { get; private set; }
    public string? MetadataJson { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public string Result { get; private set; } = null!;

    private AuditLog() { }

    public AuditLog(
        Guid? tenantId,
        Guid? userId,
        string action,
        string entityType,
        string? entityId,
        string? metadataJson,
        string? ipAddress,
        string? userAgent,
        string? correlationId,
        string result)
    {
        TenantId = tenantId;
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        MetadataJson = metadataJson;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
        Result = result;
    }
}
