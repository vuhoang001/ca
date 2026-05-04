using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Auth.Commands;

public sealed record RefreshTokenCommand(string RefreshToken, string? ClientId = null) : IRequest<TokenResponse>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MinimumLength(32);
        RuleFor(x => x.ClientId).MaximumLength(100);
    }
}

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IClientAppRepository clientAppRepository,
    ITokenService tokenService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
        var currentRefreshToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, true, cancellationToken)
            ?? throw new UnauthorizedException("Refresh token is invalid.");

        if (!currentRefreshToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is no longer active.");
        }

        ClientApp? clientApp = null;
        if (!string.IsNullOrWhiteSpace(request.ClientId))
        {
            clientApp = await clientAppRepository.GetByClientIdAsync(request.ClientId!, cancellationToken)
                ?? throw new UnauthorizedException("Unknown client application.");
        }

        var user = currentRefreshToken.User;
        var roles = await userRepository.GetRoleNamesAsync(user.Id, cancellationToken);
        var permissions = await userRepository.GetPermissionCodesAsync(user.Id, cancellationToken);
        var tokenResult = tokenService.GenerateTokens(user.Id, user.Email, user.UserName, user.TenantId, roles,
                                                      permissions, clientApp?.ClientId);

        var nextRefreshToken = new RefreshToken(
            user.Id,
            clientApp?.Id,
            tokenResult.RefreshTokenHash,
            tokenResult.JwtId,
            tokenResult.RefreshTokenExpiresAtUtc,
            request.ClientId,
            currentUserContext.IpAddress,
            currentUserContext.UserAgent);

        refreshTokenRepository.Add(nextRefreshToken);
        currentRefreshToken.Revoke("Rotated", nextRefreshToken.Id);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("auth.token.refreshed", nameof(User), user.Id.ToString(),
                                      new { clientApp = request.ClientId }, "Success", cancellationToken);

        return new TokenResponse(
            tokenResult.AccessToken,
            tokenResult.AccessTokenExpiresAtUtc,
            tokenResult.RefreshToken,
            tokenResult.RefreshTokenExpiresAtUtc,
            new UserProfileResponse(user.Id, user.Email, user.UserName, user.TenantId, roles, permissions));
    }
}