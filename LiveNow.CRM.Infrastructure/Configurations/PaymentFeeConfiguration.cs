using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class PaymentFeeConfiguration : IEntityTypeConfiguration<PaymentFee>
{
    public void Configure(EntityTypeBuilder<PaymentFee> builder)
    {
        builder.ToTable("PaymentFees");

        builder.HasKey(pf => pf.Id);

        builder.Property(pf => pf.FeeType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(pf => pf.Rate)
            .HasPrecision(18, 6);

        builder.Property(pf => pf.FixedAmount)
            .HasPrecision(18, 2);

        builder.Property(pf => pf.CalculatedAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(pf => pf.Currency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(pf => pf.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(pf => pf.PaymentId);

        // Foreign key
        builder.HasOne(pf => pf.Payment)
            .WithMany(p => p.Fees)
            .HasForeignKey(pf => pf.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
