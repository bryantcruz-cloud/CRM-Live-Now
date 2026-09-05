using FluentAssertions;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class CancellationServiceTests : ServiceTestBase, IAsyncLifetime
{
    private Customer _customer = null!;
    private RaceEdition _edition = null!;
    private RaceSlot _slot = null!;

    public async Task InitializeAsync()
    {
        _customer = await SeedCustomerAsync();
        _edition = await SeedEditionAsync();
        _slot = await SeedSlotAsync(_edition.Id);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Create_WithReassignable_Should_Set_Slot_Transferable()
    {
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);

        CancellationDto result = await CancellationService.CreateAsync(new CreateCancellationDto
        {
            CustomerId = _customer.Id,
            SaleId = sale.Id,
            Reason = "Change of plans",
            IsInjury = false,
            CanReassignSlot = true
        });

        result.Should().NotBeNull();
        result.IsInjury.Should().BeFalse();
        result.CanReassignSlot.Should().BeTrue();

        RaceSlot? storedSlot = await Context.RaceSlots.FirstAsync(s => s.Id == _slot.Id);
        storedSlot.Status.Should().Be(SlotStatusEnum.Transferable);

        Sale? storedSale = await Context.Sales.FirstAsync(s => s.Id == sale.Id);
        storedSale.Status.Should().Be(SaleStatusEnum.Cancelled);
    }

    [Fact]
    public async Task Create_Injury_Should_Register_IsInjury()
    {
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);

        CancellationDto result = await CancellationService.CreateAsync(new CreateCancellationDto
        {
            CustomerId = _customer.Id,
            SaleId = sale.Id,
            Reason = "Knee injury",
            IsInjury = true,
            CanReassignSlot = false
        });

        result.IsInjury.Should().BeTrue();

        RaceSlot? storedSlot = await Context.RaceSlots.FirstAsync(s => s.Id == _slot.Id);
        storedSlot.Status.Should().Be(SlotStatusEnum.Cancelled);
    }

    [Fact]
    public async Task Create_Without_Sale_Should_Only_Cancel_Slot()
    {
        CancellationDto result = await CancellationService.CreateAsync(new CreateCancellationDto
        {
            CustomerId = _customer.Id,
            RaceSlotId = _slot.Id,
            Reason = "Direct slot cancellation",
            CanReassignSlot = true
        });

        result.Should().NotBeNull();
        result.SaleId.Should().BeNull();
    }

    [Fact]
    public async Task Create_With_Empty_Reason_Should_Throw_Validation()
    {
        Func<Task> act = () => CancellationService.CreateAsync(new CreateCancellationDto
        {
            CustomerId = _customer.Id,
            Reason = "",
            CanReassignSlot = false
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Create_With_NonExistent_Customer_Should_Throw_NotFound()
    {
        Func<Task> act = () => CancellationService.CreateAsync(new CreateCancellationDto
        {
            CustomerId = Guid.NewGuid(),
            Reason = "Test",
            CanReassignSlot = false
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
