using Domain.Entities;

namespace Application.Abstractions;

public interface IRevokedAccessTokenRepository
{
    void Add(RevokedAccessToken token);
    Task<bool> ExistsActiveAsync(string jwtId, CancellationToken cancellationToken = default);
}