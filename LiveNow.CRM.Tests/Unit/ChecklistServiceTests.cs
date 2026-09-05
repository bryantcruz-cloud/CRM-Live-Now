using FluentAssertions;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class ChecklistServiceTests : ServiceTestBase, IAsyncLifetime
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
    public async Task Create_Should_Add_Item()
    {
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);

        ChecklistItemDto result = await ChecklistService.CreateAsync(_customer.Id, new CreateChecklistItemDto
        {
            SaleId = sale.Id,
            ItemType = "Travel",
            Description = "Book flight"
        });

        result.Should().NotBeNull();
        result.ItemType.Should().Be("Travel");
        result.Description.Should().Be("Book flight");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task Update_To_Completed_Should_Set_CompletedAt()
    {
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);
        ChecklistItemDto item = await ChecklistService.CreateAsync(_customer.Id, new CreateChecklistItemDto
        {
            SaleId = sale.Id,
            ItemType = "Travel",
            Description = "Book flight"
        });

        ChecklistItemDto result = await ChecklistService.UpdateAsync(item.Id, new UpdateChecklistItemDto
        {
            ItemType = "Travel",
            Description = "Book flight",
            IsCompleted = true
        });

        result.IsCompleted.Should().BeTrue();
        result.CompletedAt.Should().NotBeNull();

        CustomerChecklist? stored = await Context.CustomerChecklists.FirstAsync(c => c.Id == item.Id);
        stored.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_Uncompleting_Should_Clear_CompletedAt()
    {
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);
        ChecklistItemDto item = await ChecklistService.CreateAsync(_customer.Id, new CreateChecklistItemDto
        {
            SaleId = sale.Id,
            ItemType = "Travel",
            Description = "Book flight"
        });

        await ChecklistService.UpdateAsync(item.Id, new UpdateChecklistItemDto
        {
            ItemType = "Travel",
            Description = "Book flight",
            IsCompleted = true
        });

        ChecklistItemDto result = await ChecklistService.UpdateAsync(item.Id, new UpdateChecklistItemDto
        {
            ItemType = "Travel",
            Description = "Book flight updated",
            IsCompleted = false
        });

        result.IsCompleted.Should().BeFalse();
        result.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetByCustomer_Should_Return_Only_Customer_Items()
    {
        Customer otherCustomer = await SeedCustomerAsync("Other", "Person");
        Sale sale1 = await SeedSaleWithSlotAsync(_customer, _edition, await SeedSlotAsync(_edition.Id));
        Sale sale2 = await SeedSaleWithSlotAsync(otherCustomer, _edition, await SeedSlotAsync(_edition.Id));

        await ChecklistService.CreateAsync(_customer.Id, new CreateChecklistItemDto { SaleId = sale1.Id, ItemType = "Travel", Description = "Item 1" });
        await ChecklistService.CreateAsync(otherCustomer.Id, new CreateChecklistItemDto { SaleId = sale2.Id, ItemType = "Travel", Description = "Item Other" });

        IReadOnlyList<ChecklistItemDto> items = await ChecklistService.GetByCustomerAsync(_customer.Id);

        items.Should().HaveCount(1);
        items[0].CustomerId.Should().Be(_customer.Id);
    }

    [Fact]
    public async Task Create_With_Empty_Description_Should_Throw_Validation()
    {
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);

        Func<Task> act = () => ChecklistService.CreateAsync(_customer.Id, new CreateChecklistItemDto
        {
            SaleId = sale.Id,
            ItemType = "Travel",
            Description = ""
        });

        await act.Should().ThrowAsync<ValidationException>();
    }
}
