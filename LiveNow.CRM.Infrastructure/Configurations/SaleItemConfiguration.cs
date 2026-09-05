using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(si => si.Id);

        builder.Property(si => si.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(si => si.ItemType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(si => si.Quantity)
            .IsRequired();

        builder.Property(si => si.UnitCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(si => si.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(si => si.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(si => si.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(si => si.RoomType).HasMaxLength(200);
        builder.Property(si => si.ReservationPolicy).HasMaxLength(1000);
        builder.Property(si => si.BoardBasis).HasConversion<int>();

        builder.HasIndex(si => si.SaleId);
        builder.HasIndex(si => si.RaceSlotId);

        // Foreign keys
        builder.HasOne(si => si.Sale)
            .WithMany(s => s.Items)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.RaceSlot)
            .WithMany(rs => rs.SaleItems)
            .HasForeignKey(si => si.RaceSlotId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
