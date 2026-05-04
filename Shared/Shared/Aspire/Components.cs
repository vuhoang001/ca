namespace Auth.Shared.Shared.Aspire;

public static class Components
{
    public const string Queue = "queue";
    public const string SqlServer = "sqlserver";
    public const string Postgres = "postgres";
    public const string ContainerRegistry = "container-registry";
    public const string KeyCloak = "keycloak";


    public static class Database
    {
        public const string Auth     = "authdb";
        public const string Catalog  = "catalogdb";
        public const string Basket   = "basketdb";
        public const string Identity = "identitydb";
    }
}
