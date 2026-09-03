using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.City)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.Country)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(h => h.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(h => h.Name);
        builder.HasIndex(h => h.IsActive);

        // Foreign key
        builder.HasOne(h => h.Supplier)
            .WithMany(s => s.Hotels)
            .HasForeignKey(h => h.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
