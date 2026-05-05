using Domain.Entities;

namespace Application.Abstractions;

public interface IRefreshTokenRepository
{
    void Add(RefreshToken refreshToken);

    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser = false,
        CancellationToken cancellationToken = default);

    Task RevokeAllActiveByUserIdAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
}