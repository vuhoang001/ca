using Domain.Entities;

namespace Api.Application;

public interface IRevokedAccessTokenRepository
{
    void Add(RevokedAccessToken token);
    Task<bool> ExistsActiveAsync(string jwtId, CancellationToken cancellationToken = default);
}