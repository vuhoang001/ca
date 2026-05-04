using Microsoft.AspNetCore.Authorization;

namespace DownstreamApi.Security;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissions = context.User.FindAll("permissions").Select(claim => claim.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class PermissionAuthorizationExtensions
{
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        return services;
    }

    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string permission)
    {
        return builder.AddRequirements(new PermissionRequirement(permission));
    }
}

/*
builder.Services
    .AddAuthentication()
    .AddJwtBearer();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("orders.read", policy => policy.RequireAuthenticatedUser().RequirePermission("orders.read"));

builder.Services.AddPermissionAuthorization();

app.MapGet("/orders", () => Results.Ok())
    .RequireAuthorization("orders.read");
*/
