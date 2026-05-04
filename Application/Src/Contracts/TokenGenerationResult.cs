namespace Api.Application;

public sealed record TokenGenerationResult(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    string RefreshTokenHash,
    string JwtId,
    DateTime RefreshTokenExpiresAtUtc);

public sealed record AccessTokenDescriptor(string JwtId, Guid? UserId, DateTime ExpiresAtUtc);
