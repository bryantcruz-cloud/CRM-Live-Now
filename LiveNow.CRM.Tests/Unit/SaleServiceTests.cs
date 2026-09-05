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

public class SaleServiceTests : TestBase
{
    private readonly UnitOfWork _unitOfWork;
    private readonly AuditService _auditService;
    private readonly FinancialCalculatorService _financialCalculator;
    private readonly SaleService _service;
    private readonly Guid _customerId;
    private readonly Guid _editionId;

    public SaleServiceTests()
    {
        _unitOfWork = new UnitOfWork(Context);
        _auditService = new AuditService(Context);
        _financialCalculator = new FinancialCalculatorService();
        _service = new SaleService(Context, _unitOfWork, _auditService, _financialCalculator);

        Customer customer = new() { FirstName = "Sale", LastName = "Customer", Email = "sale@example.com" };
        Context.Customers.Add(customer);
        Race race = new() { Name = "Sale Race", City = "City", Country = "Country", RaceType = RaceTypeEnum.Marathon };
        Context.Races.Add(race);
        RaceEdition edition = new() { RaceId = race.Id, Year = 2032, Currency = CurrencyEnum.USD, Status = RaceEditionStatusEnum.Planning };
        Context.RaceEditions.Add(edition);
        Context.SaveChanges();
        _customerId = customer.Id;
        _editionId = edition.Id;
    }

    [Fact]
    public async Task Create_Should_Add_Sale_With_Pending_Status()
    {
        CreateSaleDto dto = new()
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Currency = CurrencyEnum.USD,
            Items = new()
            {
                new CreateSaleItemDto { Description = "Entry", ItemType = SaleItemTypeEnum.Entry, Quantity = 1, UnitCost = 500m, UnitPrice = 1000m }
            }
        };

        SaleDto result = await _service.CreateAsync(dto);

        result.Status.Should().Be(SaleStatusEnum.Pending);
        result.TotalSalePrice.Should().Be(1000m);
        result.TotalCost.Should().Be(500m);
        result.GrossProfit.Should().Be(500m);
    }

    [Fact]
    public async Task Confirm_Should_Assign_Slot_And_Set_Sold()
    {
        // Create a slot
        RaceSlot slot = new()
        {
            RaceEditionId = _editionId,
            InternalCode = "CONF-001",
            Status = SlotStatusEnum.Available,
            AcquisitionCost = 500m,
            AcquisitionCurrency = CurrencyEnum.USD
        };
        Context.RaceSlots.Add(slot);
        Context.SaveChanges();

        // Create a sale with an entry item
        SaleDto sale = await _service.CreateAsync(new CreateSaleDto
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Currency = CurrencyEnum.USD,
            Items = new()
            {
                new CreateSaleItemDto { Description = "Entry", ItemType = SaleItemTypeEnum.Entry, Quantity = 1, UnitCost = 500m, UnitPrice = 1000m }
            }
        });

        // Confirm with slot assignment
        SaleDto confirmed = await _service.ConfirmAsync(sale.Id, new ConfirmSaleDto
        {
            Slots = new() { new ConfirmSlotPriceDto { SlotId = slot.Id, UnitPrice = 1000m } }
        });

        confirmed.Status.Should().Be(SaleStatusEnum.Confirmed);

        RaceSlot? storedSlot = await Context.RaceSlots.FirstAsync(s => s.Id == slot.Id);
        storedSlot.Status.Should().Be(SlotStatusEnum.Sold);
        storedSlot.AssignedCustomerId.Should().Be(_customerId);
    }

    [Fact]
    public async Task Confirm_Sold_Slot_Should_Throw_Conflict()
    {
        // Create a sold slot
        RaceSlot slot = new()
        {
            RaceEditionId = _editionId,
            InternalCode = "SOLD-001",
            Status = SlotStatusEnum.Sold,
            AcquisitionCost = 500m,
            AcquisitionCurrency = CurrencyEnum.USD,
            AssignedCustomerId = _customerId
        };
        Context.RaceSlots.Add(slot);
        Context.SaveChanges();

        SaleDto sale = await _service.CreateAsync(new CreateSaleDto
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Currency = CurrencyEnum.USD,
            Items = new()
            {
                new CreateSaleItemDto { Description = "Entry", ItemType = SaleItemTypeEnum.Entry, Quantity = 1, UnitCost = 500m, UnitPrice = 1000m }
            }
        });

        Func<Task> act = () => _service.ConfirmAsync(sale.Id, new ConfirmSaleDto
        {
            Slots = new() { new ConfirmSlotPriceDto { SlotId = slot.Id, UnitPrice = 1000m } }
        });

        await act.Should().ThrowAsync<ConflictException>();
    }
}
