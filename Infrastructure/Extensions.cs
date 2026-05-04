using System.ComponentModel;
using Shared.Extensions.EF;
using Shared.Extensions.Repository;
using Shared.Shared.Aspire;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class Extensions
{
    public static void AddInfrastructure(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        builder.AddSqlServerEfDbContext<AppDbContext>(Components.Database.Auth,
                                                      _ => { services.AddRepositories(typeof(AppDbContext)); });
    }
}