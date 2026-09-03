using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Infrastructure.Configurations;

public class HotelReservationConfiguration : IEntityTypeConfiguration<HotelReservation>
{
    public void Configure(EntityTypeBuilder<HotelReservation> builder)
    {
        builder.ToTable("HotelReservations");

        builder.HasKey(hr => hr.Id);

        builder.Property(hr => hr.RoomType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(hr => hr.Occupancy)
            .IsRequired();

        builder.Property(hr => hr.NumberOfRooms)
            .IsRequired();

        builder.Property(hr => hr.Cost)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(hr => hr.SalePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(hr => hr.Currency)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(hr => hr.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(hr => hr.ConfirmationNumber)
            .HasMaxLength(100);

        builder.Property(hr => hr.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(hr => hr.CustomerId);
        builder.HasIndex(hr => hr.SaleId);
        builder.HasIndex(hr => hr.HotelId);
        builder.HasIndex(hr => hr.Status);

        // Foreign keys
        builder.HasOne(hr => hr.Customer)
            .WithMany(c => c.HotelReservations)
            .HasForeignKey(hr => hr.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(hr => hr.Sale)
            .WithMany(s => s.HotelReservations)
            .HasForeignKey(hr => hr.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(hr => hr.Hotel)
            .WithMany(h => h.Reservations)
            .HasForeignKey(hr => hr.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
