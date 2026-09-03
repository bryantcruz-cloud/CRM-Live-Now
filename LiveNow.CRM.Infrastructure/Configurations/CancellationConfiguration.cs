using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class CancellationConfiguration : IEntityTypeConfiguration<Cancellation>
{
    public void Configure(EntityTypeBuilder<Cancellation> builder)
    {
        builder.ToTable("Cancellations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.RefundAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.SaleId);
        builder.HasIndex(c => c.RaceSlotId);
        builder.HasIndex(c => c.CancellationDate);

        // Foreign keys
        builder.HasOne(c => c.Customer)
            .WithMany(cu => cu.Cancellations)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Sale)
            .WithMany(s => s.Cancellations)
            .HasForeignKey(c => c.SaleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.RaceSlot)
            .WithMany(rs => rs.Cancellations)
            .HasForeignKey(c => c.RaceSlotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
