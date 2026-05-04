using System.Text.Json;
using Application.Abstractions;
using Domain.Entities;
using Shared.Abstractions;

namespace Infrastructure.Auditing;

public sealed class AuditService(
    IAuditLogRepository auditLogRepository,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork) : IAuditService
{
    public async Task WriteAsync(
        string action,
        string entityType,
        string? entityId,
        object? metadata,
        string result,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog(
            currentUserContext.TenantId,
            currentUserContext.UserId,
            null,
            action,
            entityType,
            entityId,
            metadata is null ? null : JsonSerializer.Serialize(metadata),
            currentUserContext.IpAddress,
            currentUserContext.UserAgent,
            currentUserContext.CorrelationId,
            result);

        auditLogRepository.Add(auditLog);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
