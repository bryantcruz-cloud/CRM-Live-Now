using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class RaceSlotConfiguration : IEntityTypeConfiguration<RaceSlot>
{
    public void Configure(EntityTypeBuilder<RaceSlot> builder)
    {
        builder.ToTable("RaceSlots");

        builder.HasKey(rs => rs.Id);

        builder.Property(rs => rs.InternalCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(rs => rs.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(rs => rs.AcquisitionCost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(rs => rs.AcquisitionCurrency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(rs => rs.Notes)
            .HasMaxLength(2000);

        builder.Property(rs => rs.Version)
            .IsConcurrencyToken();

        // Unique constraint: InternalCode
        builder.HasIndex(rs => rs.InternalCode)
            .IsUnique();

        builder.HasIndex(rs => rs.RaceEditionId);
        builder.HasIndex(rs => rs.SupplierId);
        builder.HasIndex(rs => rs.AssignedCustomerId);
        builder.HasIndex(rs => rs.Status);

        // Foreign keys
        builder.HasOne(rs => rs.RaceEdition)
            .WithMany(re => re.Slots)
            .HasForeignKey(rs => rs.RaceEditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rs => rs.Supplier)
            .WithMany(s => s.Slots)
            .HasForeignKey(rs => rs.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(rs => rs.AssignedCustomer)
            .WithMany()
            .HasForeignKey(rs => rs.AssignedCustomerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
