var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver");
var authDb = sqlServer.AddDatabase("AuthDb");

builder.AddProject<Projects.Api>("api")
    .WithReference(authDb)
    .WaitFor(authDb);

builder.Build().Run();