namespace Auth.Shared.Shared.Aspire;

public static class Services
{
    public const string Catalog = "Catalog";
    public const string Basket = "Basket";
    public const string Identity = "Identity";
    public const string Gateway = "Gateway";



    public static string ToClientName(string application, string? suffix = null)
    {
        var clientName = char.ToUpperInvariant(application[0]) + application[1..];
        return string.IsNullOrWhiteSpace(suffix) ? clientName : $"{clientName} {suffix}";
    }
}
