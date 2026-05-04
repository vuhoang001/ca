using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(string Email, string UserName, string Password, Guid? TenantId = null)
    : IRequest<RegisteredUserResponse>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterUserCommand, RegisteredUserResponse>
{
    public async Task<RegisteredUserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim().ToUpperInvariant();
        if (await userRepository.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException($"Email '{request.Email}' already exists.");
        }

        var user = new User(request.TenantId, request.UserName, request.Email, string.Empty);
        user.SetPassword(passwordService.HashPassword(user, request.Password));

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await auditService.WriteAsync("user.registered", nameof(User), user.Id.ToString(),
                                      new { user.Email, user.UserName }, "Success", cancellationToken);

        return new RegisteredUserResponse(user.Id, user.Email, user.UserName, user.CreatedAtUtc);
    }
}