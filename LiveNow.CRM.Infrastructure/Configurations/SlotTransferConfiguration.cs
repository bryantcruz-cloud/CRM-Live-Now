using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class SlotTransferConfiguration : IEntityTypeConfiguration<SlotTransfer>
{
    public void Configure(EntityTypeBuilder<SlotTransfer> builder)
    {
        builder.ToTable("SlotTransfers");

        builder.HasKey(st => st.Id);

        builder.Property(st => st.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(st => st.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(st => st.RaceSlotId);
        builder.HasIndex(st => st.FromCustomerId);
        builder.HasIndex(st => st.ToCustomerId);
        builder.HasIndex(st => st.TransferDate);

        // Foreign key
        builder.HasOne(st => st.RaceSlot)
            .WithMany(rs => rs.SlotTransfers)
            .HasForeignKey(st => st.RaceSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
