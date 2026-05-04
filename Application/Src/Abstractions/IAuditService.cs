namespace Application.Abstractions;

public interface IAuditService
{
    Task WriteAsync(
        string action,
        string entityType,
        string? entityId,
        object? metadata,
        string result,
        CancellationToken cancellationToken = default);
}