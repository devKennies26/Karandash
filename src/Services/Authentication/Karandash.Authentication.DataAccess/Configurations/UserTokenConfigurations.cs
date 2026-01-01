using Karandash.Authentication.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Karandash.Authentication.DataAccess.Configurations;

public class UserTokenConfigurations : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pt => pt.ExpiresDate)
            .IsRequired();

        builder.HasOne(pt => pt.User)
            .WithOne(u => u.UserToken)
            .HasForeignKey<UserToken>(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}