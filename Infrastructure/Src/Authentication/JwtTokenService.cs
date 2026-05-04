using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Application;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions              _options      = options.Value;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public TokenGenerationResult GenerateTokens(
        Guid userId,
        string email,
        string userName,
        Guid? tenantId,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions,
        string? clientId)
    {
        var now       = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var jwtId     = Guid.NewGuid().ToString("N");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("preferred_username", userName),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email)
        };

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(clientId))
        {
            claims.Add(new Claim("client_id", clientId));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(permissions.Select(permission => new Claim("permissions", permission)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claims),
            Expires            = expiresAt,
            Issuer             = _options.Issuer,
            Audience           = _options.Audience,
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        var token        = _tokenHandler.CreateToken(tokenDescriptor);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new TokenGenerationResult(
            _tokenHandler.WriteToken(token),
            expiresAt,
            refreshToken,
            HashRefreshToken(refreshToken),
            jwtId,
            now.AddDays(_options.RefreshTokenDays));
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }

    public AccessTokenDescriptor ReadAccessToken(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidIssuer              = _options.Issuer,
            ValidateAudience         = true,
            ValidAudience            = _options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            ValidateLifetime         = false
        };

        var principal = _tokenHandler.ValidateToken(accessToken, validationParameters, out var validatedToken);
        if (validatedToken is not JwtSecurityToken jwtSecurityToken)
        {
            throw new SecurityTokenException("Invalid access token.");
        }

        var jwtId = principal.FindFirstValue(JwtRegisteredClaimNames.Jti) ?? principal.FindFirstValue("jti")
            ?? throw new SecurityTokenException("Missing token identifier.");
        Guid? userId =
            Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"),
                          out var parsedUserId)
                ? parsedUserId
                : null;
        var expiresAtUtc = jwtSecurityToken.ValidTo;
        return new AccessTokenDescriptor(jwtId, userId, expiresAtUtc);
    }
}