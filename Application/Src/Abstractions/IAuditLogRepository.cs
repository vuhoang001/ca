using Domain.Entities;

namespace Application.Abstractions;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
}
