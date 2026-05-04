namespace Infrastructure.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; set; } = "admin@local.dev";
    public string AdminUserName { get; set; } = "admin";
    public string AdminPassword { get; set; } = "Admin@123456";
    public string DefaultTenantName { get; set; } = "Default Tenant";
    public string DefaultTenantSlug { get; set; } = "default";
}