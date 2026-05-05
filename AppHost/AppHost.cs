var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", "YourStrong@Password123", secret: true);

var sqlServer = builder.AddSqlServer("sqlserver", password: sqlPassword)
    .WithHostPort(14388);
var authDb = sqlServer.AddDatabase("AuthDb");

builder.AddProject<Projects.Api>("api")
    .WithReference(authDb)
    .WaitFor(authDb);

builder.Build().Run();