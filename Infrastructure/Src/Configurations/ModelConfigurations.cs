using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.NormalizedEmail).IsUnique();
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.NormalizedName }).IsUnique();
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Resource).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ConditionsJson).HasMaxLength(2000);
        // unique per (tenant, code) for tenant permissions
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        // unique code among system permissions (TenantId IS NULL)
        builder.HasIndex(x => x.Code).IsUnique().HasFilter("[TenantId] IS NULL");
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.Property(x => x.AssignedBy).HasMaxLength(256);
        builder.HasOne(x => x.User).WithMany(x => x.UserRoles).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.Role).WithMany(x => x.UserRoles).HasForeignKey(x => x.RoleId);
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.Property(x => x.AssignedBy).HasMaxLength(256);
        builder.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId);
        builder.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId);
    }
}

internal sealed class ClientAppConfiguration : IEntityTypeConfiguration<ClientApp>
{
    public void Configure(EntityTypeBuilder<ClientApp> builder)
    {
        builder.ToTable("client_apps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecretHash).HasMaxLength(500);
        builder.Property(x => x.AllowedScopes).HasMaxLength(1000);
        builder.Property(x => x.AllowedOrigins).HasMaxLength(2000);
        builder.HasIndex(x => x.ClientId).IsUnique();
    }
}

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.JwtId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RevocationReason).HasMaxLength(200);
        builder.Property(x => x.DeviceName).HasMaxLength(100);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });
        builder.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId);
        builder.HasOne(x => x.ClientApp).WithMany().HasForeignKey(x => x.ClientAppId).OnDelete(DeleteBehavior.SetNull);
    }
}

internal sealed class RevokedAccessTokenConfiguration : IEntityTypeConfiguration<RevokedAccessToken>
{
    public void Configure(EntityTypeBuilder<RevokedAccessToken> builder)
    {
        builder.ToTable("revoked_access_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.JwtId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.JwtId).IsUnique();
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EntityId).HasMaxLength(64);
        builder.Property(x => x.MetadataJson).HasMaxLength(2000);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.Result).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
    }
}