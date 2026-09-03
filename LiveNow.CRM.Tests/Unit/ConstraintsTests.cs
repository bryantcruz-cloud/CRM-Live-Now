using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Entities;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class ConstraintsTests : TestBase
{
    [Fact]
    public async Task RaceSlot_DuplicateInternalCode_ShouldFail()
    {
        Race race = new() { Name = "Test Race", City = "Test City", Country = "Test Country" };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new() { RaceId = race.Id, Year = 2026 };
        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        RaceSlot slot1 = new()
        {
            RaceEditionId = edition.Id,
            InternalCode = "DUPLICATE-001",
            AcquisitionCost = 250.00m
        };

        RaceSlot slot2 = new()
        {
            RaceEditionId = edition.Id,
            InternalCode = "DUPLICATE-001",
            AcquisitionCost = 300.00m
        };

        Context.RaceSlots.Add(slot1);
        await Context.SaveChangesAsync();

        Context.RaceSlots.Add(slot2);
        await Assert.ThrowsAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }

    [Fact]
    public async Task RaceEdition_DuplicateRaceYear_ShouldFail()
    {
        Race race = new() { Name = "Test Race", City = "Test City", Country = "Test Country" };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition1 = new() { RaceId = race.Id, Year = 2026 };
        RaceEdition edition2 = new() { RaceId = race.Id, Year = 2026 };

        Context.RaceEditions.Add(edition1);
        await Context.SaveChangesAsync();

        Context.RaceEditions.Add(edition2);
        await Assert.ThrowsAsync<DbUpdateException>(() => Context.SaveChangesAsync());
    }
}
