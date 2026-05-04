using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Auth.EventHandlers;

public sealed class UserRegisteredDomainEventHandler(ILogger<UserRegisteredDomainEventHandler> logger)
    : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handled domain event {EventName} for user {UserId} with email {Email}",
            nameof(UserRegisteredDomainEvent),
            notification.UserId,
            notification.Email);

        return Task.CompletedTask;
    }
}
