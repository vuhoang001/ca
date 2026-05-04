using Shared.Helpers;

namespace Auth.Shared.Extensions.EventBus;

public abstract record IntegrationEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime CreationDate { get; } = DateTimeHelper.UtcNow();
}
