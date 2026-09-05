using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> builder)
    {
        builder.ToTable("QuoteItems");

        builder.HasKey(qi => qi.Id);

        builder.Property(qi => qi.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(qi => qi.ItemType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(qi => qi.Quantity)
            .IsRequired();

        builder.Property(qi => qi.UnitCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(qi => qi.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(qi => qi.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(qi => qi.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(qi => qi.Notes)
            .HasMaxLength(1000);

        builder.Property(qi => qi.RoomType).HasMaxLength(200);
        builder.Property(qi => qi.ReservationPolicy).HasMaxLength(1000);
        builder.Property(qi => qi.BoardBasis).HasConversion<int>();

        builder.HasIndex(qi => qi.QuoteId);

        // Foreign key
        builder.HasOne(qi => qi.Quote)
            .WithMany(q => q.Items)
            .HasForeignKey(qi => qi.QuoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
