using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public sealed class AuditLogRepository(AppDbContext dbContext) : IAuditLogRepository
{
    public void Add(AuditLog auditLog) => dbContext.AuditLogs.Add(auditLog);
}
