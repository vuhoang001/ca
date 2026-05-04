using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Auth.Shared.Extensions.EF;

public static class DbContextExtensions
{
    public static void AddSqlServerEfDbContext<TDbContext>(
        this IHostApplicationBuilder builder,
        string name,
        Action<IHostApplicationBuilder>? action = null,
        bool excludeDefaultInterceptors = false
    )
        where TDbContext : DbContext
    {
        var services = builder.Services;
        
        
        var connectionString = builder.Configuration.GetConnectionString(name);

        if (!excludeDefaultInterceptors)
        {
            services.AddScoped<ISaveChangesInterceptor, EventDispatchInterceptor>();
            services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
        }


        services.AddDbContext<TDbContext>((sp, options) =>
        {
            options
                .UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(TDbContext).Assembly.FullName);
                        sql.EnableRetryOnFailure();
                    });
            // .ConfigureWarnings(warnings =>
            //                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning)
            // );


            var interceptors = sp.GetServices<ISaveChangesInterceptor>()
                .Cast<IInterceptor>()
                .ToArray();

            if (interceptors.Length != 0)
            {
                options.AddInterceptors(interceptors);
            }
        });


        action?.Invoke(builder);
    }
}