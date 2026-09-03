using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class FinancialTests : TestBase
{
    [Fact]
    public async Task CreateSale_ShouldPersistInDatabase()
    {
        Customer customer = new() { FirstName = "Bob", LastName = "Johnson", Email = "bob@example.com" };
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        Race race = new() { Name = "Test Race", City = "Test City", Country = "Test Country" };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new() { RaceId = race.Id, Year = 2026 };
        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        Sale sale = new()
        {
            SaleNumber = "S-2026-001",
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            Currency = CurrencyEnum.USD,
            SaleDate = DateTime.UtcNow,
            Subtotal = 1000.00m,
            Discount = 0.00m,
            Taxes = 160.00m,
            TotalSalePrice = 1160.00m,
            TotalCost = 250.00m,
            TotalPaymentFees = 35.00m,
            GrossProfit = 875.00m,
            ProfitMargin = 0.7543m,
            Status = SaleStatusEnum.Pending
        };

        Context.Sales.Add(sale);
        await Context.SaveChangesAsync();

        Sale? savedSale = await Context.Sales.FirstOrDefaultAsync(s => s.Id == sale.Id);
        savedSale.Should().NotBeNull();
        savedSale!.SaleNumber.Should().Be("S-2026-001");
        savedSale.TotalSalePrice.Should().Be(1160.00m);
        savedSale.GrossProfit.Should().Be(875.00m);
    }

    [Fact]
    public async Task RegisterMultiplePayments_ShouldCalculatePendingBalance()
    {
        Customer customer = new() { FirstName = "Alice", LastName = "Williams", Email = "alice@example.com" };
        Context.Customers.Add(customer);
        await Context.SaveChangesAsync();

        Race race = new() { Name = "Test Race", City = "Test City", Country = "Test Country" };
        Context.Races.Add(race);
        await Context.SaveChangesAsync();

        RaceEdition edition = new() { RaceId = race.Id, Year = 2026 };
        Context.RaceEditions.Add(edition);
        await Context.SaveChangesAsync();

        Sale sale = new()
        {
            SaleNumber = "S-2026-002",
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            Currency = CurrencyEnum.USD,
            TotalSalePrice = 1000.00m,
            TotalCost = 250.00m,
            TotalPaymentFees = 0.00m,
            GrossProfit = 750.00m,
            Status = SaleStatusEnum.PartiallyPaid
        };
        Context.Sales.Add(sale);
        await Context.SaveChangesAsync();

        Payment payment1 = new()
        {
            SaleId = sale.Id,
            Amount = 400.00m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Card,
            Status = PaymentStatusEnum.Completed
        };

        Payment payment2 = new()
        {
            SaleId = sale.Id,
            Amount = 300.00m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.BankTransfer,
            Status = PaymentStatusEnum.Completed
        };

        Context.Payments.AddRange(payment1, payment2);
        await Context.SaveChangesAsync();

        Sale? savedSale = await Context.Sales
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.Id == sale.Id);
        savedSale.Should().NotBeNull();

        decimal totalPaid = savedSale!.Payments
            .Where(p => p.Status == PaymentStatusEnum.Completed)
            .Sum(p => p.Amount);

        decimal pendingBalance = savedSale.TotalSalePrice - totalPaid;

        totalPaid.Should().Be(700.00m);
        pendingBalance.Should().Be(300.00m);
    }

    [Fact]
    public void CalculateCardCommission_ShouldReturnCorrectAmount()
    {
        Payment payment = new()
        {
            Amount = 1000.00m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Card,
            Status = PaymentStatusEnum.Completed
        };

        PaymentFee fee = new()
        {
            PaymentId = payment.Id,
            FeeType = PaymentFeeTypeEnum.PercentagePlusFixed,
            Rate = 0.035m,
            FixedAmount = 0.30m,
            Currency = CurrencyEnum.USD
        };

        fee.CalculatedAmount = (payment.Amount * fee.Rate.Value) + fee.FixedAmount.Value;

        fee.CalculatedAmount.Should().Be(35.30m);
    }

    [Fact]
    public void CalculateGrossProfit_ShouldReturnCorrectAmount()
    {
        decimal totalSalePrice = 1160.00m;
        decimal totalCost = 250.00m;
        decimal totalPaymentFees = 35.00m;

        decimal grossProfit = totalSalePrice - totalCost - totalPaymentFees;
        decimal profitMargin = grossProfit / totalSalePrice;

        grossProfit.Should().Be(875.00m);
        profitMargin.Should().BeApproximately(0.7543m, 0.0001m);
    }
}
