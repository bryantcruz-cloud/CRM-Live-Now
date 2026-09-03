using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Infrastructure.Data;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Seed Races
        var chicagoId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var berlinId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        modelBuilder.Entity<Race>().HasData(
            new Race
            {
                Id = chicagoId,
                Name = "Chicago Marathon",
                City = "Chicago",
                Country = "United States",
                RaceType = RaceTypeEnum.Marathon,
                Status = RaceStatusEnum.Active,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new Race
            {
                Id = berlinId,
                Name = "Berlin Marathon",
                City = "Berlin",
                Country = "Germany",
                RaceType = RaceTypeEnum.Marathon,
                Status = RaceStatusEnum.Active,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );

        // Seed Race Editions
        var chicago2026Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012");
        var berlin2026Id = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123");

        modelBuilder.Entity<RaceEdition>().HasData(
            new RaceEdition
            {
                Id = chicago2026Id,
                RaceId = chicagoId,
                Year = 2026,
                RaceDate = new DateTime(2026, 10, 11, 0, 0, 0, DateTimeKind.Utc),
                Currency = CurrencyEnum.USD,
                Status = RaceEditionStatusEnum.Open,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            },
            new RaceEdition
            {
                Id = berlin2026Id,
                RaceId = berlinId,
                Year = 2026,
                RaceDate = new DateTime(2026, 9, 27, 0, 0, 0, DateTimeKind.Utc),
                Currency = CurrencyEnum.EUR,
                Status = RaceEditionStatusEnum.Open,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );
    }
}
