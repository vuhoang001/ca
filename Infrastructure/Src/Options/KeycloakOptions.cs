namespace Infrastructure.Options;

public sealed class KeycloakOptions
{
    public const string Section = "Keycloak";
    public string Authority { get; init; } = "";
    public string Audience { get; init; } = "";
    public string ClientId { get; init; } = "";
    public bool RequireHttpsMetadata { get; init; } = true;
}
