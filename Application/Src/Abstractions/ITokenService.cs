namespace Api.Application;

public interface ITokenService
{
    TokenGenerationResult GenerateTokens(
        Guid userId,
        string email,
        string userName,
        Guid? tenantId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions,
        string? clientId);

    string HashRefreshToken(string refreshToken);

    AccessTokenDescriptor ReadAccessToken(string accessToken);
}