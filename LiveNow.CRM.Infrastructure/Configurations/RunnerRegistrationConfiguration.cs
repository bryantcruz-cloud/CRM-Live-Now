using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class RunnerRegistrationConfiguration : IEntityTypeConfiguration<RunnerRegistration>
{
    public void Configure(EntityTypeBuilder<RunnerRegistration> builder)
    {
        builder.ToTable("RunnerRegistrations");

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.RegistrationStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(rr => rr.ConfirmationNumber)
            .HasMaxLength(100);

        builder.Property(rr => rr.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(rr => rr.CustomerId);
        builder.HasIndex(rr => rr.RaceEditionId);
        builder.HasIndex(rr => rr.RaceSlotId);
        builder.HasIndex(rr => rr.RegistrationStatus);

        // Foreign keys
        builder.HasOne(rr => rr.Customer)
            .WithMany(c => c.RunnerRegistrations)
            .HasForeignKey(rr => rr.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rr => rr.RaceEdition)
            .WithMany(re => re.RunnerRegistrations)
            .HasForeignKey(rr => rr.RaceEditionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rr => rr.RaceSlot)
            .WithMany(rs => rs.RunnerRegistrations)
            .HasForeignKey(rr => rr.RaceSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
