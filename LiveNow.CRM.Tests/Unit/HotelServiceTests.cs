using FluentAssertions;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class HotelServiceTests : ServiceTestBase, IAsyncLifetime
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
    public async Task Create_Should_Add_Hotel()
    {
        HotelDto result = await HotelService.CreateAsync(new CreateHotelDto
        {
            Name = "Grand Hotel Test",
            City = "Chicago",
            Country = "USA"
        });

        result.Should().NotBeNull();
        result.Name.Should().Be("Grand Hotel Test");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Create_With_Empty_Name_Should_Throw_Validation()
    {
        Func<Task> act = () => HotelService.CreateAsync(new CreateHotelDto
        {
            Name = "",
            City = "Chicago",
            Country = "USA"
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateReservation_Should_Calculate_Nights()
    {
        Hotel hotel = await SeedHotelAsync();
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);

        HotelReservationDto result = await HotelService.CreateReservationAsync(new CreateHotelReservationDto
        {
            CustomerId = _customer.Id,
            SaleId = sale.Id,
            HotelId = hotel.Id,
            CheckIn = new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc),
            RoomType = "Double",
            Occupancy = 2,
            NumberOfRooms = 1,
            Cost = 600m,
            SalePrice = 750m,
            Currency = CurrencyEnum.USD
        });

        result.Should().NotBeNull();
        result.Nights.Should().Be(3);
        result.Status.Should().Be(HotelReservationStatusEnum.Pending);
    }

    [Fact]
    public async Task CreateReservation_With_CheckOut_Before_CheckIn_Should_Throw_Validation()
    {
        Hotel hotel = await SeedHotelAsync();
        Sale sale = await SeedSaleWithSlotAsync(_customer, _edition, _slot);

        Func<Task> act = () => HotelService.CreateReservationAsync(new CreateHotelReservationDto
        {
            CustomerId = _customer.Id,
            SaleId = sale.Id,
            HotelId = hotel.Id,
            CheckIn = new DateTime(2026, 10, 13, 0, 0, 0, DateTimeKind.Utc),
            CheckOut = new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc),
            RoomType = "Double",
            Occupancy = 2,
            NumberOfRooms = 1,
            Cost = 600m,
            SalePrice = 750m
        });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetAll_Should_Return_Active_Hotels()
    {
        await SeedHotelAsync("Hotel A");
        await SeedHotelAsync("Hotel B");

        IReadOnlyList<HotelDto> hotels = await HotelService.GetAllAsync();

        hotels.Should().HaveCountGreaterOrEqualTo(2);
    }
}
