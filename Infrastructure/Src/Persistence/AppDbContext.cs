using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Abstractions;
using Shared.Kernel;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserContext currentUserContext) : DbContext(options), IUnitOfWork
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Product> Products => Set<Product>();
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
                entry.Entity.CreatedBy ??= currentUserContext.Email ?? currentUserContext.Username ?? "system";
            }

            if (entry.State is EntityState.Modified or EntityState.Added)
            {
                entry.Entity.LastModifiedAtUtc = dateTimeProvider.UtcNow;
                entry.Entity.LastModifiedBy = currentUserContext.Email ?? currentUserContext.Username ?? "system";
            }
        }
    }
}
