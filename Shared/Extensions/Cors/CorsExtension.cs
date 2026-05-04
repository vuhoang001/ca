using Shared.Shared.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Auth.Shared.Extensions.Cors;

public static class CorsExtension
{
    private const string AllowAllCorsPolicy             = "AllowAll";
    private const string AllowSpecificOriginsCorsPolicy = "AllowSpecific";


    public static void AddDefaultCors(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowAllCorsPolicy,
                    policyBuilder =>
                    {
                        policyBuilder
                            .SetIsOriginAllowed(origin => new Uri(origin).Host == Network.Localhost)
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    }
                );
            });
        }
        else
        {
            var corsConfiguration = builder.Configuration.GetSection(CorsSetting.ConfigurationSection);
            services.Configure<CorsSetting>(corsConfiguration);

            services.AddCors(options =>
            {
                options.AddPolicy(
                    AllowSpecificOriginsCorsPolicy, policyBuilder =>
                    {
                        var serviceProvider = services.BuildServiceProvider();
                        var corsOptions     = serviceProvider.GetRequiredService<CorsSetting>();

                        policyBuilder
                            .WithOrigins([.. corsOptions.Origins])
                            .WithHeaders([.. corsOptions.Headers])
                            .WithMethods([.. corsOptions.Methods]);

                        if (corsOptions.MaxAge is not null)
                        {
                            policyBuilder.SetPreflightMaxAge(
                                TimeSpan.FromSeconds(corsOptions.MaxAge.Value));
                        }

                        if (corsOptions.AllowCredentials)
                        {
                            policyBuilder.AllowCredentials();
                        }
                    }
                );
            });
        }
    }
}