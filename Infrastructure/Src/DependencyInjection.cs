using System.Text;
using Api.Application;
using Api.Infrastructure;
using Application.Abstractions;
using Infrastructure.Auditing;
using Infrastructure.Authentication;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));

        var connectionString = configuration.GetConnectionString("AuthDb") ??
            throw new InvalidOperationException("Connection string 'AuthDb' not found.");

        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var interceptors = provider.GetServices<ISaveChangesInterceptor>().ToArray<IInterceptor>();

            options.UseSqlServer(connectionString,
                                 builder => builder.MigrationsAssembly(
                                     typeof(AppDbContext).Assembly.FullName));

            if (interceptors.Length != 0)
            {
                options.AddInterceptors(interceptors);
            }
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IClientAppRepository, ClientAppRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IRevokedAccessTokenRepository, RevokedAccessTokenRepository>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<DbSeeder>();

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtOptions.Issuer,
                    ValidateAudience         = true,
                    ValidAudience            = jwtOptions.Audience,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ClockSkew                = TimeSpan.FromSeconds(30)
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var revokedRepository = context.HttpContext.RequestServices
                            .GetRequiredService<IRevokedAccessTokenRepository>();
                        var jwtId = context.Principal?.FindFirst("jti")?.Value;

                        if (!string.IsNullOrWhiteSpace(jwtId) &&
                            await revokedRepository.ExistsActiveAsync(jwtId, context.HttpContext.RequestAborted))
                        {
                            context.Fail("Token has been revoked.");
                        }
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                                   .RequireAuthenticatedUser()
                                   .Build());

        return services;
    }
}