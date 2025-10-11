using Karandash.Shared.Utils.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Karandash.Authentication.DataAccess.Configurations;

public class OutboxEventConfigurations : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("OutboxEvents");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.ProcessedAt)
            .IsRequired(false);

        builder.Property(o => o.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(o => new { o.ProcessedAt, o.RetryCount });
    }
}