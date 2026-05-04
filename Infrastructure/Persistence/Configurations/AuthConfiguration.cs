using Domain.Entities;
using Shared.Shared.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public class AuthConfiguration : IEntityTypeConfiguration<Domain.Entities.Auth>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Auth> builder)
    {
        builder.ToTable(nameof(AppDbContext.Auths));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Email)
            .HasMaxLength(DataSchemaLength.ExtraLarge)
            .IsRequired();

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(DataSchemaLength.SuperLarge)
            .IsRequired();

        builder.Property(x => x.EmailConfirmed)
            .HasMaxLength(DataSchemaLength.Small)
            .IsRequired();

        builder.Property(x => x.SecurityStamp)
            .HasMaxLength(DataSchemaLength.ExtraLarge)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion(
                status => status.Value,
                value => ConvertToAuthStatus(value))
            .HasMaxLength(DataSchemaLength.Small)
            .IsRequired();
    }

    private static AuthStatus ConvertToAuthStatus(string value)
    {
        return value switch
        {
            "Active"  => AuthStatus.Active,
            "Locked"  => AuthStatus.Locked,
            "Deleted" => AuthStatus.Deleted,
            _         => throw new InvalidOperationException($"Unknown AuthStatus: {value}")
        };
    }
}