using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Auth.Commands;

public sealed record LogoutCommand(string? RefreshToken = null) : IRequest;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRevokedAccessTokenRepository revokedAccessTokenRepository,
    ITokenService tokenService,
    IAuditService auditService,
    ICurrentUserContext currentUserContext,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserContext.UserId ?? throw new UnauthorizedException("Authentication required.");

        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
            var refreshToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, false, cancellationToken);
            if (refreshToken is not null && refreshToken.UserId == userId && refreshToken.RevokedAtUtc is null)
                refreshToken.Revoke("User logout");
        }

        if (currentUserContext.JwtId is not null && currentUserContext.AccessTokenExpiresAt is not null &&
            !await revokedAccessTokenRepository.ExistsActiveAsync(currentUserContext.JwtId, cancellationToken))
        {
            revokedAccessTokenRepository.Add(new RevokedAccessToken(
                currentUserContext.JwtId, userId, currentUserContext.AccessTokenExpiresAt.Value, "User logout"));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await auditService.WriteAsync("auth.logout", nameof(User), userId.ToString(), null, "Success",
                                      cancellationToken);
        return Unit.Value;
    }
}
