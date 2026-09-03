using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.QuoteNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(q => q.Currency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(q => q.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(q => q.Discount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(q => q.Taxes)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(q => q.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(q => q.Notes)
            .HasMaxLength(2000);

        // Unique constraint: QuoteNumber
        builder.HasIndex(q => q.QuoteNumber)
            .IsUnique();

        builder.HasIndex(q => q.CustomerId);
        builder.HasIndex(q => q.RaceEditionId);
        builder.HasIndex(q => q.Status);

        // Foreign keys
        builder.HasOne(q => q.Customer)
            .WithMany(c => c.Quotes)
            .HasForeignKey(q => q.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.RaceEdition)
            .WithMany(re => re.Quotes)
            .HasForeignKey(q => q.RaceEditionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
