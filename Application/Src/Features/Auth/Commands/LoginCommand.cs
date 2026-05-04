using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password, string? ClientId = null) : IRequest<TokenResponse>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.ClientId).MaximumLength(100);
    }
}

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IClientAppRepository clientAppRepository,
    IPasswordService passwordService,
    ITokenService tokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IAuditService auditService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByNormalizedEmailAsync(request.Email.Trim().ToUpperInvariant(), true,
                                                                  cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.");

        if (!passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            await auditService.WriteAsync("auth.login", nameof(User), user.Id.ToString(), new { request.Email },
                                          "Failed", cancellationToken);
            throw new UnauthorizedException("Invalid credentials.");
        }

        ClientApp? clientApp = null;
        if (!string.IsNullOrWhiteSpace(request.ClientId))
        {
            clientApp = await clientAppRepository.GetByClientIdAsync(request.ClientId!, cancellationToken)
                ?? throw new UnauthorizedException("Unknown client application.");
        }

        var roles = await userRepository.GetRoleNamesAsync(user.Id, cancellationToken);
        var permissions = await userRepository.GetPermissionCodesAsync(user.Id, cancellationToken);
        var tokenResult = tokenService.GenerateTokens(user.Id, user.Email, user.UserName, user.TenantId, roles,
                                                      permissions, clientApp?.ClientId);

        var refreshToken = new RefreshToken(
            user.Id,
            clientApp?.Id,
            tokenResult.RefreshTokenHash,
            tokenResult.JwtId,
            tokenResult.RefreshTokenExpiresAtUtc,
            request.ClientId,
            currentUserContext.IpAddress,
            currentUserContext.UserAgent);

        refreshTokenRepository.Add(refreshToken);
        user.MarkLogin(dateTimeProvider.UtcNow);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("auth.login", nameof(User), user.Id.ToString(),
                                      new { user.Email, clientApp = request.ClientId }, "Success", cancellationToken);

        return new TokenResponse(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAtUtc,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAtUtc,
            new UserProfileResponse(user.Id, user.Email, user.UserName, user.TenantId, roles, permissions));
    }
}