using Karandash.Authentication.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Karandash.Authentication.DataAccess.Configurations;

public class PasswordTokenConfiguration : IEntityTypeConfiguration<PasswordToken>
{
    public void Configure(EntityTypeBuilder<PasswordToken> builder)
    {
        builder.ToTable("PasswordTokens");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pt => pt.ExpiresDate)
            .IsRequired();

        builder.HasOne(pt => pt.User)
            .WithOne(u => u.PasswordToken)
            .HasForeignKey<PasswordToken>(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}