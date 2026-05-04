using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Abstractions;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUserContext) : DbContext(options), IUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ClientApp> ClientApps => Set<ClientApp>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RevokedAccessToken> RevokedAccessTokens => Set<RevokedAccessToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = dateTimeProvider.UtcNow;
                entry.Entity.CreatedBy ??= currentUserContext.Email ?? "system";
            }

            if (entry.State is EntityState.Modified or EntityState.Added)
            {
                entry.Entity.LastModifiedAtUtc = dateTimeProvider.UtcNow;
                entry.Entity.LastModifiedBy = currentUserContext.Email ?? "system";
            }
        }
    }
}