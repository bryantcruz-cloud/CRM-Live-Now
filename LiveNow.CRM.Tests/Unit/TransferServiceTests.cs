using FluentAssertions;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class TransferServiceTests : ServiceTestBase
{
    private readonly Customer _fromCustomer;
    private readonly Customer _toCustomer;
    private readonly RaceEdition _edition;
    private readonly RaceSlot _slot;

    public TransferServiceTests()
    {
        _fromCustomer = SeedCustomerAsync("From", "Customer").GetAwaiter().GetResult();
        _toCustomer = SeedCustomerAsync("To", "Customer").GetAwaiter().GetResult();
        _edition = SeedEditionAsync().GetAwaiter().GetResult();
        _slot = SeedSlotAsync(_edition.Id, status: SlotStatusEnum.Transferable).GetAwaiter().GetResult();

        // Assign the slot to the from-customer so the transfer has a source
        _slot.AssignedCustomerId = _fromCustomer.Id;
        _slot.Status = SlotStatusEnum.Transferable;
        Context.SaveChanges();
    }

    [Fact]
    public async Task Transfer_Should_Assign_Slot_To_Target_Customer()
    {
        SlotTransferDto result = await TransferService.TransferSlotAsync(_slot.Id, new SlotTransferRequestDto
        {
            ToCustomerId = _toCustomer.Id,
            Reason = "Client no longer attends"
        });

        result.Should().NotBeNull();
        result.FromCustomerId.Should().Be(_fromCustomer.Id);
        result.ToCustomerId.Should().Be(_toCustomer.Id);
        result.RaceSlotId.Should().Be(_slot.Id);

        RaceSlot? storedSlot = await Context.RaceSlots.FirstAsync(s => s.Id == _slot.Id);
        storedSlot.AssignedCustomerId.Should().Be(_toCustomer.Id);
        storedSlot.Status.Should().Be(SlotStatusEnum.Reserved);
    }

    [Fact]
    public async Task Transfer_Should_Preserve_History()
    {
        await TransferService.TransferSlotAsync(_slot.Id, new SlotTransferRequestDto
        {
            ToCustomerId = _toCustomer.Id,
            Reason = "First transfer"
        });

        // Transfer back to original customer
        RaceSlot? slot = await Context.RaceSlots.FirstAsync(s => s.Id == _slot.Id);
        slot.Status = SlotStatusEnum.Transferable;
        slot.AssignedCustomerId = _toCustomer.Id;
        await Context.SaveChangesAsync();

        await TransferService.TransferSlotAsync(_slot.Id, new SlotTransferRequestDto
        {
            ToCustomerId = _fromCustomer.Id,
            Reason = "Second transfer"
        });

        List<SlotTransfer> history = await Context.SlotTransfers
            .Where(t => t.RaceSlotId == _slot.Id)
            .OrderBy(t => t.TransferDate)
            .ToListAsync();

        history.Should().HaveCount(2);
        history[0].ToCustomerId.Should().Be(_toCustomer.Id);
        history[1].ToCustomerId.Should().Be(_fromCustomer.Id);
    }

    [Fact]
    public async Task Transfer_Same_Customer_Should_Throw_Conflict()
    {
        Func<Task> act = () => TransferService.TransferSlotAsync(_slot.Id, new SlotTransferRequestDto
        {
            ToCustomerId = _fromCustomer.Id,
            Reason = "Same customer"
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Transfer_Sold_Slot_Should_Throw_Conflict()
    {
        _slot.Status = SlotStatusEnum.Sold;
        await Context.SaveChangesAsync();

        Func<Task> act = () => TransferService.TransferSlotAsync(_slot.Id, new SlotTransferRequestDto
        {
            ToCustomerId = _toCustomer.Id,
            Reason = "Should fail"
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Transfer_With_Empty_Reason_Should_Throw_Validation()
    {
        Func<Task> act = () => TransferService.TransferSlotAsync(_slot.Id, new SlotTransferRequestDto
        {
            ToCustomerId = _toCustomer.Id,
            Reason = ""
        });

        await act.Should().ThrowAsync<ValidationException>();
    }
}
