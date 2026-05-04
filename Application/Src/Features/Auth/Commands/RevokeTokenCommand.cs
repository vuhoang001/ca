using Api.Application;
using Application.Abstractions;
using Domain.Entities;
using FluentValidation;
using MediatR;
using Shared;
using Shared.Abstractions;
using Shared.Exceptions;

namespace Application.Features.Auth.Commands;

public sealed record RevokeTokenCommand(string? RefreshToken, string? AccessToken, string Reason = "Revoked by request")
    : IRequest;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.RefreshToken) || !string.IsNullOrWhiteSpace(x.AccessToken))
            .WithMessage("Either refresh token or access token must be provided.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(200);
    }
}

public sealed class RevokeTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRevokedAccessTokenRepository revokedAccessTokenRepository,
    ITokenService tokenService,
    IAuditService auditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RevokeTokenCommand>
{
    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);
            var refreshToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, true, cancellationToken)
                ?? throw new NotFoundException("Refresh token not found.");

            if (refreshToken.RevokedAtUtc is null)
            {
                refreshToken.Revoke(request.Reason);
            }

            await auditService.WriteAsync("auth.token.revoked", nameof(User), refreshToken.UserId.ToString(),
                                          new { kind = "refresh" }, "Success", cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(request.AccessToken))
        {
            var accessToken = tokenService.ReadAccessToken(request.AccessToken);
            if (!await revokedAccessTokenRepository.ExistsActiveAsync(accessToken.JwtId, cancellationToken))
            {
                revokedAccessTokenRepository.Add(
                    new RevokedAccessToken(accessToken.JwtId, accessToken.UserId, accessToken.ExpiresAtUtc,
                                           request.Reason));
            }

            await auditService.WriteAsync("auth.token.revoked", nameof(User), accessToken.UserId?.ToString(),
                                          new { kind = "access", accessToken.JwtId }, "Success", cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}