using FluentAssertions;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class RegistrationServiceTests : ServiceTestBase, IAsyncLifetime
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
    public async Task Create_Should_Set_Pending_Status()
    {
        RegistrationDto result = await RegistrationService.CreateAsync(new CreateRegistrationDto
        {
            CustomerId = _customer.Id,
            RaceEditionId = _edition.Id,
            RaceSlotId = _slot.Id
        });

        result.Should().NotBeNull();
        result.RegistrationStatus.Should().Be(RegistrationStatusEnum.Pending);
    }

    [Fact]
    public async Task Update_To_Completed_Should_Set_RegistrationDate()
    {
        RunnerRegistration registration = await SeedRegistrationAsync(_customer, _edition, _slot);

        RegistrationDto result = await RegistrationService.UpdateAsync(registration.Id, new UpdateRegistrationDto
        {
            RegistrationStatus = RegistrationStatusEnum.Completed,
            ConfirmationNumber = "CONF-12345"
        });

        result.RegistrationStatus.Should().Be(RegistrationStatusEnum.Completed);
        result.ConfirmationNumber.Should().Be("CONF-12345");

        RunnerRegistration? stored = await Context.RunnerRegistrations.FirstAsync(r => r.Id == registration.Id);
        stored.RegistrationDate.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_Cancelled_To_Completed_Should_Throw_Conflict()
    {
        RunnerRegistration registration = await SeedRegistrationAsync(_customer, _edition, _slot);

        await RegistrationService.UpdateAsync(registration.Id, new UpdateRegistrationDto
        {
            RegistrationStatus = RegistrationStatusEnum.Cancelled
        });

        Func<Task> act = () => RegistrationService.UpdateAsync(registration.Id, new UpdateRegistrationDto
        {
            RegistrationStatus = RegistrationStatusEnum.Completed
        });

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Create_With_Wrong_Edition_Should_Throw_Conflict()
    {
        RaceEdition otherEdition = await SeedEditionAsync("Other Race", 2030);

        Func<Task> act = () => RegistrationService.CreateAsync(new CreateRegistrationDto
        {
            CustomerId = _customer.Id,
            RaceEditionId = otherEdition.Id,
            RaceSlotId = _slot.Id
        });

        await act.Should().ThrowAsync<ConflictException>();
    }
}
