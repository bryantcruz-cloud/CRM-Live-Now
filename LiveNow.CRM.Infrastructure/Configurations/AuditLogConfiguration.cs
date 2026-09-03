using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.UserId)
            .HasMaxLength(100);

        builder.Property(al => al.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.Action)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(al => al.OldValues)
            .HasMaxLength(4000);

        builder.Property(al => al.NewValues)
            .HasMaxLength(4000);

        builder.Property(al => al.Timestamp)
            .IsRequired();

        builder.HasIndex(al => al.EntityName);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.Timestamp);
        builder.HasIndex(al => al.Action);
    }
}
