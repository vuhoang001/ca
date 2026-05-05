var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", "YourStrong@Password123", secret: true);

var sqlServer = builder.AddSqlServer("sqlserver", password: sqlPassword)
    .WithHostPort(14388);
var masterDataDb = sqlServer.AddDatabase("MasterDataDb");

var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.1")
    .WithHttpEndpoint(port: 8181, targetPort: 8080, name: "http")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithEnvironment("KC_DB", "dev-file")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithArgs("start-dev", "--import-realm")
    .WithBindMount("../keycloak/realm-export.json", "/opt/keycloak/data/import/realm-export.json", isReadOnly: true);

builder.AddProject<Projects.Api>("api")
    .WithReference(masterDataDb)
    .WaitFor(masterDataDb)
    .WithEnvironment("Keycloak__Authority", $"http://localhost:8181/realms/masterdata")
    .WithEnvironment("Keycloak__RequireHttpsMetadata", "false");

builder.Build().Run();
