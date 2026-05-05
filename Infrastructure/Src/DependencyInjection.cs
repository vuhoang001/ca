using Application.Abstractions;
using Infrastructure.Auditing;
using Infrastructure.Keycloak;
using Infrastructure.Messaging;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shared.Abstractions;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();
        services.AddScoped<ISaveChangesInterceptor, EventDispatchInterceptor>();

        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.Section));
        var keycloakOptions = configuration.GetSection(KeycloakOptions.Section).Get<KeycloakOptions>() ?? new KeycloakOptions();

        var connectionString = configuration.GetConnectionString("MasterDataDb")
            ?? throw new InvalidOperationException("Connection string 'MasterDataDb' not found.");

        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var interceptors = provider.GetServices<ISaveChangesInterceptor>().ToArray<IInterceptor>();
            options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
            if (interceptors.Length != 0)
                options.AddInterceptors(interceptors);
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IEventBus, MassTransitEventBus>();

        var rabbitMqOptions = configuration.GetSection(RabbitMqOptions.Section).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        services.AddMassTransit(x =>
        {
            if (!rabbitMqOptions.IsConfigured)
            {
                // Dev/test: không cần broker, message được route trong process
                x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
            }
            else
            {
                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(rabbitMqOptions.Host, rabbitMqOptions.Port, rabbitMqOptions.VirtualHost, h =>
                    {
                        h.Username(rabbitMqOptions.Username);
                        h.Password(rabbitMqOptions.Password);
                    });
                    cfg.ConfigureEndpoints(ctx);
                });
            }
        });

        services.AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                opts.Authority = keycloakOptions.Authority;
                opts.Audience = keycloakOptions.Audience;
                opts.RequireHttpsMetadata = keycloakOptions.RequireHttpsMetadata;
                opts.TokenValidationParameters.ValidateLifetime = true;
                opts.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(30);
            });

        services
            .AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());

        return services;
    }
}
