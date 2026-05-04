using Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Authentication;

public static class PermissionPolicyExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string permission)
    {
        return builder.AddRequirements(new PermissionRequirement(permission));
    }
}
