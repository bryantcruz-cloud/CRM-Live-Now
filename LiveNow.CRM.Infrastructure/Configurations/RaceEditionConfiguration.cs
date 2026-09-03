using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class RaceEditionConfiguration : IEntityTypeConfiguration<RaceEdition>
{
    public void Configure(EntityTypeBuilder<RaceEdition> builder)
    {
        builder.ToTable("RaceEditions");

        builder.HasKey(re => re.Id);

        builder.Property(re => re.Year)
            .IsRequired();

        builder.Property(re => re.Currency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(re => re.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(re => re.Notes)
            .HasMaxLength(2000);

        // Unique constraint: Race + Year
        builder.HasIndex(re => new { re.RaceId, re.Year })
            .IsUnique();

        builder.HasIndex(re => re.Status);

        // Foreign key
        builder.HasOne(re => re.Race)
            .WithMany(r => r.Editions)
            .HasForeignKey(re => re.RaceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
