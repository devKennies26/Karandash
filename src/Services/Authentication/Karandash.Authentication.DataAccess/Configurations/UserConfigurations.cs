using Karandash.Authentication.Core.Entities;
using Karandash.Shared.Enums.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Karandash.Authentication.DataAccess.Configurations;

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.PasswordSalt)
            .IsRequired();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PendingEmail)
            .HasMaxLength(255);

        builder.Property(u => u.IsVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.RefreshToken)
            .HasMaxLength(500);

        builder.Property(u => u.UserRole)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        builder.HasData(new User()
        {
            Id = Guid.Parse("88B608C2-94A6-4C7D-9D55-B128018CAB4E"),

            InsertedAt = DateTime.UtcNow,
            UpdatedAt = null,
            RemovedAt = null,

            FirstName = "System",
            LastName = "Administrator",

            Email = "info.karandashmmc@gmail.com",
            PendingEmail = null,

            PasswordSalt = Convert.FromBase64String("6m1PBsQHAHrzvbDJYrPx6A=="),
            PasswordHash = "gmr+kg55gqEC0RJIlql4CKFHsB2uWTXubsgPiZLr/qU=",

            UserRole = UserRole.Admin,

            IsVerified = true,

            RefreshToken = null,
            RefreshTokenExpireDate = null,

            IsDeleted = false
        });
    }
}