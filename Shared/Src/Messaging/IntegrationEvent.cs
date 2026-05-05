namespace Shared.Messaging;

/// <summary>
/// Base class for all integration events published to external message brokers.
/// </summary>
public abstract class IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; } = DateTime.UtcNow;

    /// <summary>
    /// Tracing ID to correlate the integration event back to the originating HTTP request.
    /// </summary>
    public string? CorrelationId { get; init; }
}
