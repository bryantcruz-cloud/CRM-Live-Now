using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class RelationshipsTests : TestBase
{
    [Fact]
    public async Task VerifyMainRelationships_ShouldWorkCorrectly()
    {
        Race race = new()
        {
            Name = "Relationship Race Test Marathon",
            City = "Chicago",
            Country = "United States",
            RaceType = RaceTypeEnum.Marathon
        };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new()
        {
            RaceId = race.Id,
            Year = 2026,
            Currency = CurrencyEnum.USD,
            Status = RaceEditionStatusEnum.Open
        };
        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        Supplier supplier = new()
        {
            Name = "Official Race Supplier",
            ContactName = "Contact Person"
        };
        Context.Suppliers.Add(supplier);
        await Context.SaveChangesAsync();

        RaceSlot slot = new()
        {
            RaceEditionId = edition.Id,
            InternalCode = "CHI-2026-001",
            AcquisitionCost = 250.00m,
            AcquisitionCurrency = CurrencyEnum.USD,
            SupplierId = supplier.Id,
            Status = SlotStatusEnum.Available
        };
        Context.RaceSlots.Add(slot);
        await Context.SaveChangesAsync();

        Customer customer = new()
        {
            FirstName = "Test",
            LastName = "Customer",
            Email = "test@example.com"
        };
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        Quote quote = new()
        {
            QuoteNumber = "Q-2026-001",
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            Currency = CurrencyEnum.USD,
            Subtotal = 1000.00m,
            Total = 1160.00m
        };
        Context.Quotes.Add(quote);
        await Context.SaveChangesAsync();

        Sale sale = new()
        {
            SaleNumber = "S-2026-001",
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            QuoteId = quote.Id,
            Currency = CurrencyEnum.USD,
            TotalSalePrice = 1160.00m,
            TotalCost = 250.00m,
            TotalPaymentFees = 35.00m,
            GrossProfit = 875.00m,
            ProfitMargin = 0.7543m
        };
        Context.Sales.Add(sale);
        await Context.SaveChangesAsync();

        SaleItem saleItem = new()
        {
            SaleId = sale.Id,
            RaceSlotId = slot.Id,
            Description = "Race Entry",
            ItemType = SaleItemTypeEnum.Entry,
            Quantity = 1,
            UnitCost = 250.00m,
            UnitPrice = 1160.00m,
            TotalCost = 250.00m,
            TotalPrice = 1160.00m
        };
        Context.SaleItems.Add(saleItem);
        await Context.SaveChangesAsync();

        Payment payment = new()
        {
            SaleId = sale.Id,
            Amount = 500.00m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Card,
            Status = PaymentStatusEnum.Completed
        };
        Context.Payments.Add(payment);
        await Context.SaveChangesAsync();

        Sale? savedSale = await Context.Sales
            .Include(s => s.Customer)
            .Include(s => s.RaceEdition)
            .ThenInclude(re => re.Race)
            .Include(s => s.Quote)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == sale.Id);

        savedSale.Should().NotBeNull();
        savedSale!.Customer.FirstName.Should().Be("Test");
        savedSale.RaceEdition.Race.Name.Should().Be("Relationship Race Test Marathon");
        savedSale.Quote!.QuoteNumber.Should().Be("Q-2026-001");
        savedSale.Items.Should().HaveCount(1);
        savedSale.Items.First().RaceSlotId.Should().Be(slot.Id);
        savedSale.Payments.Should().HaveCount(1);
    }
}
