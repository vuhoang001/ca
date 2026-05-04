namespace Infrastructure.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "auth-service";
    public string Audience { get; set; } = "auth-clients";
    public string SigningKey { get; set; } = "change-this-super-secret-key-change-this";
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
}
