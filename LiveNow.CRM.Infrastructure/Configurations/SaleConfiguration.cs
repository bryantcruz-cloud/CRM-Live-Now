using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SaleNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Currency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.SaleDate)
            .IsRequired();

        builder.Property(s => s.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Discount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Taxes)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.TotalSalePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.TotalCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.TotalPaymentFees)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.GrossProfit)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.ProfitMargin)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.Notes)
            .HasMaxLength(2000);

        // Unique constraint: SaleNumber
        builder.HasIndex(s => s.SaleNumber)
            .IsUnique();

        builder.HasIndex(s => s.CustomerId);
        builder.HasIndex(s => s.RaceEditionId);
        builder.HasIndex(s => s.QuoteId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.SaleDate);

        // Foreign keys
        builder.HasOne(s => s.Customer)
            .WithMany(c => c.Sales)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.RaceEdition)
            .WithMany(re => re.Sales)
            .HasForeignKey(s => s.RaceEditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Quote)
            .WithMany(q => q.Sales)
            .HasForeignKey(s => s.QuoteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
