using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class DomainModelTests : TestBase
{
    [Fact]
    public async Task CreateCustomer_ShouldPersistInDatabase()
    {
        Customer customer = new()
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Phone = "+1234567890",
            Country = "United States",
            City = "New York",
            Status = CustomerStatusEnum.Active
        };

        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        Customer? savedCustomer = await Context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.FirstName.Should().Be("John");
        savedCustomer.LastName.Should().Be("Doe");
        savedCustomer.Email.Should().Be("john.doe@example.com");
        savedCustomer.Status.Should().Be(CustomerStatusEnum.Active);
        savedCustomer.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateRace_ShouldPersistInDatabase()
    {
        Race race = new()
        {
            Name = "Create Race Test Marathon",
            City = "Chicago",
            Country = "United States",
            RaceType = RaceTypeEnum.Marathon,
            Status = RaceStatusEnum.Active
        };

        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        Race? savedRace = await Context.Races.FirstOrDefaultAsync(r => r.Id == race.Id);
        savedRace.Should().NotBeNull();
        savedRace!.Name.Should().Be("Create Race Test Marathon");
        savedRace.RaceType.Should().Be(RaceTypeEnum.Marathon);
    }

    [Fact]
    public async Task CreateRaceEdition_ShouldPersistInDatabase()
    {
        Race race = new()
        {
            Name = "Edition Race Test Marathon",
            City = "Berlin",
            Country = "Germany",
            RaceType = RaceTypeEnum.Marathon,
            Status = RaceStatusEnum.Active
        };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new()
        {
            RaceId = race.Id,
            Year = 2026,
            RaceDate = new DateTime(2026, 9, 27),
            Currency = CurrencyEnum.EUR,
            Status = RaceEditionStatusEnum.Open
        };

        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        RaceEdition? savedEdition = await Context.RaceEditions.FirstOrDefaultAsync(e => e.Id == edition.Id);
        savedEdition.Should().NotBeNull();
        savedEdition!.RaceId.Should().Be(race.Id);
        savedEdition.Year.Should().Be(2026);
        savedEdition.Currency.Should().Be(CurrencyEnum.EUR);
    }

    [Fact]
    public async Task CreateRaceSlot_ShouldPersistInDatabase()
    {
        Race race = new() { Name = "Test Race", City = "Test City", Country = "Test Country" };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new() { RaceId = race.Id, Year = 2026 };
        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        RaceSlot slot = new()
        {
            RaceEditionId = edition.Id,
            InternalCode = "CHI-2026-001",
            Status = SlotStatusEnum.Available,
            AcquisitionCost = 250.00m,
            AcquisitionCurrency = CurrencyEnum.USD
        };

        Context.RaceSlots.Add(slot);
        await Context.SaveChangesAsync();

        RaceSlot? savedSlot = await Context.RaceSlots.FirstOrDefaultAsync(s => s.Id == slot.Id);
        savedSlot.Should().NotBeNull();
        savedSlot!.InternalCode.Should().Be("CHI-2026-001");
        savedSlot.AcquisitionCost.Should().Be(250.00m);
        savedSlot.Status.Should().Be(SlotStatusEnum.Available);
    }

    [Fact]
    public async Task CreateQuote_ShouldPersistInDatabase()
    {
        Customer customer = new() { FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" };
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        Race race = new() { Name = "Test Race", City = "Test City", Country = "Test Country" };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new() { RaceId = race.Id, Year = 2026 };
        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        Quote quote = new()
        {
            QuoteNumber = "Q-2026-001",
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            Status = QuoteStatusEnum.Draft,
            Currency = CurrencyEnum.USD,
            Subtotal = 1000.00m,
            Discount = 50.00m,
            Taxes = 152.00m,
            Total = 1102.00m
        };

        Context.Quotes.Add(quote);
        await Context.SaveChangesAsync();

        Quote? savedQuote = await Context.Quotes.FirstOrDefaultAsync(q => q.Id == quote.Id);
        savedQuote.Should().NotBeNull();
        savedQuote!.QuoteNumber.Should().Be("Q-2026-001");
        savedQuote.Total.Should().Be(1102.00m);
    }
}
