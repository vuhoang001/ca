using Api.Application;
using Application.Common;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Options;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure;

public sealed class DbSeeder(AppDbContext dbContext, IOptions<SeedOptions> seedOptions)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var options = seedOptions.Value;
        var tenant = new Tenant(options.DefaultTenantName, options.DefaultTenantSlug);
        dbContext.Tenants.Add(tenant);

        var permissions = PermissionCodes.All
            .Select(code =>
            {
                var parts = code.Split('.');
                return new Permission(code, code, parts.Length > 1 ? parts[1] : "auth", parts[^1],
                                      $"Seeded permission {code}");
            })
            .ToList();

        dbContext.Permissions.AddRange(permissions);

        var adminRole = new Role(tenant.Id, "Administrator", "System administrator", true);
        var appUserRole = new Role(tenant.Id, "User", "Default user role");
        dbContext.Roles.AddRange(adminRole, appUserRole);

        dbContext.RolePermissions.AddRange(
            permissions.Select(permission => new RolePermission(adminRole.Id, permission.Id, "seed")));

        var admin = new User(tenant.Id, options.AdminUserName, options.AdminEmail, string.Empty, true);
        admin.SetPassword(new PasswordService().HashPassword(admin, options.AdminPassword));
        admin.Activate();
        dbContext.Users.Add(admin);

        dbContext.UserRoles.Add(new UserRole(admin.Id, adminRole.Id, "seed"));

        dbContext.ClientApps.Add(new ClientApp(tenant.Id, "default-web", "Default Web App", ClientAppType.Public));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}