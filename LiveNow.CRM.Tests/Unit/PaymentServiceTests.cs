using FluentAssertions;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class PaymentServiceTests : TestBase
{
    private readonly UnitOfWork _unitOfWork;
    private readonly AuditService _auditService;
    private readonly FinancialCalculatorService _financialCalculator;
    private readonly PaymentFeeCalculator _feeCalculator;
    private readonly SaleService _saleService;
    private readonly PaymentService _service;
    private readonly Guid _customerId;
    private readonly Guid _editionId;

    public PaymentServiceTests()
    {
        _unitOfWork = new UnitOfWork(Context);
        _auditService = new AuditService(Context);
        _financialCalculator = new FinancialCalculatorService();
        _feeCalculator = new PaymentFeeCalculator();
        _saleService = new SaleService(Context, _unitOfWork, _auditService, _financialCalculator);
        _service = new PaymentService(Context, _unitOfWork, _auditService, _financialCalculator, _feeCalculator);

        Customer customer = new() { FirstName = "Pay", LastName = "Customer", Email = "pay@example.com" };
        Context.Customers.Add(customer);
        Race race = new() { Name = "Pay Race", City = "City", Country = "Country", RaceType = RaceTypeEnum.Marathon };
        Context.Races.Add(race);
        RaceEdition edition = new() { RaceId = race.Id, Year = 2033, Currency = CurrencyEnum.USD, Status = RaceEditionStatusEnum.Planning };
        Context.RaceEditions.Add(edition);
        Context.SaveChanges();
        _customerId = customer.Id;
        _editionId = edition.Id;
    }

    private async Task<SaleDto> CreateConfirmedSaleAsync()
    {
        SaleDto sale = await _saleService.CreateAsync(new CreateSaleDto
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Currency = CurrencyEnum.USD,
            Items = new()
            {
                new CreateSaleItemDto { Description = "Entry", ItemType = SaleItemTypeEnum.Entry, Quantity = 1, UnitCost = 500m, UnitPrice = 1000m }
            }
        });

        return await _saleService.ConfirmAsync(sale.Id, new ConfirmSaleDto());
    }

    [Fact]
    public async Task RegisterPayment_Should_Calculate_Outstanding_Balance()
    {
        SaleDto sale = await CreateConfirmedSaleAsync();

        await _service.RegisterPaymentAsync(sale.Id, new CreatePaymentDto
        {
            Amount = 700m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Card
        });

        SaleFinancialSummaryDto summary = await _service.GetFinancialSummaryAsync(sale.Id);

        summary.SaleTotal.Should().Be(1000m);
        summary.PaidAmount.Should().Be(700m);
        summary.OutstandingBalance.Should().Be(300m);
    }

    [Fact]
    public async Task RegisterPayment_Should_Calculate_Fee_And_Profit()
    {
        SaleDto sale = await CreateConfirmedSaleAsync();

        await _service.RegisterPaymentAsync(sale.Id, new CreatePaymentDto
        {
            Amount = 1000m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Card,
            Fee = new CreatePaymentFeeDto
            {
                FeeType = PaymentFeeTypeEnum.PercentagePlusFixed,
                Rate = 3.5m,
                FixedAmount = 0.30m
            }
        });

        SaleFinancialSummaryDto summary = await _service.GetFinancialSummaryAsync(sale.Id);

        // Fee = 1000 * 3.5% + 0.30 = 35.30
        summary.PaymentFees.Should().Be(35.30m);
        // GrossProfit = 1000 - 500 - 35.30 = 464.70
        summary.GrossProfit.Should().Be(464.70m);
        summary.OutstandingBalance.Should().Be(0m);
    }

    [Fact]
    public async Task RegisterPayment_Negative_Amount_Should_Throw_Validation()
    {
        SaleDto sale = await CreateConfirmedSaleAsync();

        Func<Task> act = () => _service.RegisterPaymentAsync(sale.Id, new CreatePaymentDto
        {
            Amount = -100m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Cash
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RegisterPayment_Exceeding_Balance_Should_Throw_Validation()
    {
        SaleDto sale = await CreateConfirmedSaleAsync();

        Func<Task> act = () => _service.RegisterPaymentAsync(sale.Id, new CreatePaymentDto
        {
            Amount = 1500m,
            Currency = CurrencyEnum.USD,
            PaymentMethod = PaymentMethodEnum.Cash
        });

        await act.Should().ThrowAsync<ValidationException>();
    }
}
