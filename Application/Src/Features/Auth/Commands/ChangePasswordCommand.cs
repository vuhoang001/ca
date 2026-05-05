using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Auth.Commands;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : IRequest;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MinimumLength(8);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IRefreshTokenRepository refreshTokenRepository,
    IRevokedAccessTokenRepository revokedAccessTokenRepository,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId ?? throw new UnauthorizedException("Authentication required.");
        var user = await userRepository.GetByIdAsync(userId, cancellationToken: cancellationToken)
            ?? throw new NotFoundException("User not found.");

        if (!passwordService.VerifyPassword(user, user.PasswordHash, request.CurrentPassword))
            throw new UnauthorizedException("Current password is invalid.");

        user.SetPassword(passwordService.HashPassword(user, request.NewPassword));

        await refreshTokenRepository.RevokeAllActiveByUserIdAsync(userId, "Password changed", cancellationToken);

        if (currentUserContext.JwtId is not null && currentUserContext.AccessTokenExpiresAt is not null &&
            !await revokedAccessTokenRepository.ExistsActiveAsync(currentUserContext.JwtId, cancellationToken))
        {
            revokedAccessTokenRepository.Add(new RevokedAccessToken(
                currentUserContext.JwtId, userId, currentUserContext.AccessTokenExpiresAt.Value, "Password changed"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("auth.password.changed", nameof(User), user.Id.ToString(),
                                      null, "Success", cancellationToken);
        return Unit.Value;
    }
}