using Shared.Helpers;
using MediatR;

namespace Auth.Shared.Primitives;

public class DomainEvent : INotification
{
    public DateTime DateOccurred { get; protected set; } = DateTimeHelper.UtcNow();
}