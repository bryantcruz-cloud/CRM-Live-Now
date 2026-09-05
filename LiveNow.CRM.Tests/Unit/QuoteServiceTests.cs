using FluentAssertions;
using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LiveNow.CRM.Tests.Unit;

public class QuoteServiceTests : TestBase
{
    private readonly UnitOfWork _unitOfWork;
    private readonly AuditService _auditService;
    private readonly QuoteService _service;
    private readonly Guid _customerId;
    private readonly Guid _editionId;

    public QuoteServiceTests()
    {
        _unitOfWork = new UnitOfWork(Context);
        _auditService = new AuditService(Context);
        _service = new QuoteService(Context, _unitOfWork, _auditService);

        Customer customer = new() { FirstName = "Quote", LastName = "Customer", Email = "quote@example.com" };
        Context.Customers.Add(customer);
        Race race = new() { Name = "Quote Race", City = "City", Country = "Country", RaceType = RaceTypeEnum.Marathon };
        Context.Races.Add(race);
        RaceEdition edition = new() { RaceId = race.Id, Year = 2031, Currency = CurrencyEnum.USD, Status = RaceEditionStatusEnum.Planning };
        Context.RaceEditions.Add(edition);
        Context.SaveChanges();
        _customerId = customer.Id;
        _editionId = edition.Id;
    }

    [Fact]
    public async Task Create_Should_Calculate_Totals()
    {
        CreateQuoteDto dto = new()
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Currency = CurrencyEnum.USD,
            Discount = 50m,
            Taxes = 30m,
            Items = new()
            {
                new CreateQuoteItemDto { Description = "Entry", ItemType = QuoteItemTypeEnum.Entry, Quantity = 1, UnitCost = 500m, UnitPrice = 1000m },
                new CreateQuoteItemDto { Description = "Hotel", ItemType = QuoteItemTypeEnum.Hotel, Quantity = 2, UnitCost = 200m, UnitPrice = 300m, HotelId = SeedHotelId(), CheckIn = new DateTime(2031, 10, 1), CheckOut = new DateTime(2031, 10, 3), RoomType = "Doble", NumberOfRooms = 1, Occupancy = 2, BoardBasis = HotelBoardBasisEnum.Breakfast }
            }
        };

        QuoteDto result = await _service.CreateAsync(dto);

        // Subtotal = 1000 + 600 = 1600; Total = 1600 - 50 + 30 = 1580
        result.Subtotal.Should().Be(1600m);
        result.Total.Should().Be(1580m);
        result.Status.Should().Be(QuoteStatusEnum.Draft);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Send_Should_Change_Status_To_Sent()
    {
        QuoteDto quote = await CreateDraft();

        QuoteDto sent = await _service.SendAsync(quote.Id);

        sent.Status.Should().Be(QuoteStatusEnum.Sent);
    }

    [Fact]
    public async Task Accept_Should_Change_Status_To_Accepted()
    {
        QuoteDto quote = await CreateDraft();
        await _service.SendAsync(quote.Id);

        QuoteDto accepted = await _service.AcceptAsync(quote.Id);

        accepted.Status.Should().Be(QuoteStatusEnum.Accepted);
    }

    [Fact]
    public async Task Create_HotelItem_WithInvalidDates_Should_Fail()
    {
        Guid hotelId = SeedHotelId();
        CreateQuoteDto dto = new()
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Items = [new CreateQuoteItemDto { Description = "Hotel", ItemType = QuoteItemTypeEnum.Hotel, HotelId = hotelId, CheckIn = new DateTime(2031, 10, 3), CheckOut = new DateTime(2031, 10, 3), RoomType = "Doble", NumberOfRooms = 1, Occupancy = 2, BoardBasis = HotelBoardBasisEnum.RoomOnly }]
        };

        Func<Task> act = () => _service.CreateAsync(dto);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*check-out*posterior*");
    }

    [Fact]
    public async Task CreateAndConvert_HotelItem_Should_PersistNightsAndHotelDetailsInSale()
    {
        Guid hotelId = SeedHotelId();
        QuoteDto created = await _service.CreateAsync(new CreateQuoteDto
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Items = [new CreateQuoteItemDto { Description = "Hotel stay", ItemType = QuoteItemTypeEnum.Hotel, HotelId = hotelId, CheckIn = new DateTime(2031, 11, 1), CheckOut = new DateTime(2031, 11, 4), RoomType = "Suite", NumberOfRooms = 2, Occupancy = 3, BoardBasis = HotelBoardBasisEnum.HalfBoard, ReservationPolicy = "No reembolso" }]
        });

        created.Items.Single().Nights.Should().Be(3);
        await _service.SendAsync(created.Id);
        await _service.AcceptAsync(created.Id);
        await _service.ConvertToSaleAsync(created.Id, new ConvertQuoteToSaleDto());

        SaleItem item = await Context.SaleItems.SingleAsync();
        item.HotelId.Should().Be(hotelId);
        item.Nights.Should().Be(3);
        item.RoomType.Should().Be("Suite");
        item.NumberOfRooms.Should().Be(2);
        item.Occupancy.Should().Be(3);
        item.BoardBasis.Should().Be(HotelBoardBasisEnum.HalfBoard);
        item.ReservationPolicy.Should().Be("No reembolso");
    }

    [Fact]
    public async Task ConvertToSale_AcceptedQuote_Should_CreateConfirmedSaleAndSellAssignedSlot()
    {
        Quote quote = await SeedAcceptedQuoteAsync();
        RaceSlot slot = await SeedSlotAsync("AVAILABLE-001");

        QuoteDto result = await _service.ConvertToSaleAsync(quote.Id, new ConvertQuoteToSaleDto
        {
            SlotAssignments = [new ConvertSlotAssignmentDto { SlotId = slot.Id, UnitPrice = 200m }]
        });

        result.Status.Should().Be(QuoteStatusEnum.ConvertedToSale);
        Sale sale = await Context.Sales.Include(s => s.Items).SingleAsync(s => s.QuoteId == quote.Id);
        sale.Status.Should().Be(SaleStatusEnum.Confirmed);
        sale.Items.Should().ContainSingle(item => item.RaceSlotId == slot.Id);
        (await Context.RaceSlots.SingleAsync(s => s.Id == slot.Id)).Status.Should().Be(SlotStatusEnum.Sold);
    }

    [Fact]
    public async Task ConvertToSale_SameQuoteTwice_Should_FailWithoutCreatingSecondSale()
    {
        Quote quote = await SeedAcceptedQuoteAsync();
        RaceSlot firstSlot = await SeedSlotAsync("AVAILABLE-002");
        RaceSlot secondSlot = await SeedSlotAsync("AVAILABLE-003");

        await _service.ConvertToSaleAsync(quote.Id, Assignment(firstSlot));

        Func<Task> act = () => _service.ConvertToSaleAsync(quote.Id, Assignment(secondSlot));

        await act.Should().ThrowAsync<ConflictException>();
        (await Context.Sales.CountAsync(s => s.QuoteId == quote.Id)).Should().Be(1);
        (await Context.RaceSlots.SingleAsync(s => s.Id == secondSlot.Id)).Status.Should().Be(SlotStatusEnum.Available);
    }

    [Fact]
    public async Task ConvertToSale_WhenRequiredSlotIsNotAvailable_Should_Fail()
    {
        Quote quote = await SeedAcceptedQuoteAsync();
        RaceSlot slot = await SeedSlotAsync("SOLD-REQUIRED", SlotStatusEnum.Sold);

        Func<Task> act = () => _service.ConvertToSaleAsync(quote.Id, Assignment(slot));

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*no está disponible*");
        (await Context.Sales.CountAsync(s => s.QuoteId == quote.Id)).Should().Be(0);
        (await Context.Quotes.SingleAsync(q => q.Id == quote.Id)).Status.Should().Be(QuoteStatusEnum.Accepted);
    }

    [Fact]
    public async Task ConvertToSale_WhenOneRequiredSlotFails_Should_RollBackSaleAndEarlierSlotChanges()
    {
        Quote quote = await SeedAcceptedQuoteAsync(twoEntryItems: true);
        RaceSlot availableSlot = await SeedSlotAsync("AVAILABLE-004");
        RaceSlot unavailableSlot = await SeedSlotAsync("SOLD-REQUIRED-005", SlotStatusEnum.Sold);

        Func<Task> act = () => _service.ConvertToSaleAsync(quote.Id, new ConvertQuoteToSaleDto
        {
            SlotAssignments =
            [
                new ConvertSlotAssignmentDto { SlotId = availableSlot.Id, UnitPrice = 200m },
                new ConvertSlotAssignmentDto { SlotId = unavailableSlot.Id, UnitPrice = 200m }
            ]
        });

        await act.Should().ThrowAsync<ConflictException>();
        (await Context.Sales.CountAsync(s => s.QuoteId == quote.Id)).Should().Be(0);
        (await Context.Quotes.AsNoTracking().SingleAsync(q => q.Id == quote.Id)).Status.Should().Be(QuoteStatusEnum.Accepted);
        RaceSlot unchangedSlot = await Context.RaceSlots.AsNoTracking().SingleAsync(s => s.Id == availableSlot.Id);
        unchangedSlot.Status.Should().Be(SlotStatusEnum.Available);
        unchangedSlot.AssignedCustomerId.Should().BeNull();
        unchangedSlot.SaleDate.Should().BeNull();
        unchangedSlot.Version.Should().Be(0);
    }

    [Fact]
    public async Task ConvertToSale_ConcurrentlyForSameSlot_Should_AllowAtMostOneSuccessfulSale()
    {
        Quote quote = await SeedAcceptedQuoteAsync();
        RaceSlot slot = await SeedSlotAsync("AVAILABLE-CONCURRENT");
        string connectionString = Context.Database.GetDbConnection().ConnectionString;

        await using SqliteConnection secondConnection = new(connectionString);
        await secondConnection.OpenAsync();
        DbContextOptions<LiveNowDbContext> secondOptions = new DbContextOptionsBuilder<LiveNowDbContext>()
            .UseSqlite(secondConnection)
            .Options;
        await using LiveNowDbContext secondContext = new(secondOptions);
        QuoteService secondService = new(secondContext, new UnitOfWork(secondContext), new AuditService(secondContext));

        Task<QuoteDto> first = _service.ConvertToSaleAsync(quote.Id, Assignment(slot));
        Task<QuoteDto> second = secondService.ConvertToSaleAsync(quote.Id, Assignment(slot));
        Task all = Task.WhenAll(first, second);

        Func<Task> act = async () => await all;
        await act.Should().ThrowAsync<Exception>();
        new[] { first, second }.Count(task => task.Status == TaskStatus.RanToCompletion).Should().Be(1);
        (await Context.Sales.CountAsync(s => s.QuoteId == quote.Id)).Should().Be(1);
        (await Context.RaceSlots.SingleAsync(s => s.Id == slot.Id)).Status.Should().Be(SlotStatusEnum.Sold);
    }

    private async Task<Quote> SeedAcceptedQuoteAsync(bool twoEntryItems = false)
    {
        QuoteDto created = await CreateDraft(twoEntryItems);
        await _service.SendAsync(created.Id);
        await _service.AcceptAsync(created.Id);
        return await Context.Quotes.Include(q => q.Items).SingleAsync(q => q.Id == created.Id);
    }

    private async Task<RaceSlot> SeedSlotAsync(string internalCode, SlotStatusEnum status = SlotStatusEnum.Available)
    {
        RaceSlot slot = new()
        {
            RaceEditionId = _editionId,
            InternalCode = internalCode,
            Status = status,
            AcquisitionCost = 75m
        };
        Context.RaceSlots.Add(slot);
        await Context.SaveChangesAsync();
        return slot;
    }

    private static ConvertQuoteToSaleDto Assignment(RaceSlot slot) => new()
    {
        SlotAssignments = [new ConvertSlotAssignmentDto { SlotId = slot.Id, UnitPrice = 200m }]
    };

    private async Task<QuoteDto> CreateDraft(bool twoEntryItems = false)
    {
        CreateQuoteDto dto = new()
        {
            CustomerId = _customerId,
            RaceEditionId = _editionId,
            Currency = CurrencyEnum.USD,
            Items = new()
            {
                new CreateQuoteItemDto { Description = "Entry", ItemType = QuoteItemTypeEnum.Entry, Quantity = 1, UnitCost = 100m, UnitPrice = 200m }
            }
        };

        if (twoEntryItems)
        {
            dto.Items.Add(new CreateQuoteItemDto
            {
                Description = "Second entry",
                ItemType = QuoteItemTypeEnum.Entry,
                Quantity = 1,
                UnitCost = 100m,
                UnitPrice = 200m
            });
        }

        return await _service.CreateAsync(dto);
    }

    private Guid SeedHotelId()
    {
        Hotel hotel = new() { Name = $"Hotel {Guid.NewGuid():N}", City = "City", Country = "Country" };
        Context.Hotels.Add(hotel);
        Context.SaveChanges();
        return hotel.Id;
    }
}
