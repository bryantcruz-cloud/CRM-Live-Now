using LiveNow.CRM.API.Services;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.Tests.Unit;

/// <summary>
/// Base class for business-service tests: builds all services on a real
/// in-memory SQLite database and provides seed helpers.
/// </summary>
public abstract class ServiceTestBase : TestBase
{
    protected IUnitOfWork UnitOfWork { get; }
    protected IAuditService AuditService { get; }
    protected IFinancialCalculator FinancialCalculator { get; }
    protected IPaymentFeeCalculator FeeCalculator { get; }
    protected CustomerService CustomerService { get; }
    protected RaceService RaceService { get; }
    protected RaceEditionService RaceEditionService { get; }
    protected RaceSlotService RaceSlotService { get; }
    protected QuoteService QuoteService { get; }
    protected SaleService SaleService { get; }
    protected PaymentService PaymentService { get; }
    protected HotelService HotelService { get; }
    protected RegistrationService RegistrationService { get; }
    protected ChecklistService ChecklistService { get; }
    protected CancellationService CancellationService { get; }
    protected TransferService TransferService { get; }

    protected ServiceTestBase()
    {
        UnitOfWork = new UnitOfWork(Context);
        AuditService = new AuditService(Context);
        FinancialCalculator = new FinancialCalculatorService();
        FeeCalculator = new PaymentFeeCalculator();
        CustomerService = new CustomerService(Context, UnitOfWork, AuditService);
        RaceService = new RaceService(Context, UnitOfWork, AuditService);
        RaceEditionService = new RaceEditionService(Context, UnitOfWork, AuditService);
        RaceSlotService = new RaceSlotService(Context, UnitOfWork, AuditService);
        QuoteService = new QuoteService(Context, UnitOfWork, AuditService);
        SaleService = new SaleService(Context, UnitOfWork, AuditService, FinancialCalculator);
        PaymentService = new PaymentService(Context, UnitOfWork, AuditService, FinancialCalculator, FeeCalculator);
        HotelService = new HotelService(Context, UnitOfWork, AuditService);
        RegistrationService = new RegistrationService(Context, UnitOfWork, AuditService);
        ChecklistService = new ChecklistService(Context, UnitOfWork, AuditService);
        CancellationService = new CancellationService(Context, UnitOfWork, AuditService);
        TransferService = new TransferService(Context, UnitOfWork, AuditService);
    }

    protected async Task<Customer> SeedCustomerAsync(string firstName = "John", string lastName = "Doe")
    {
        CustomerDto created = await CustomerService.CreateAsync(new CreateCustomerDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}.{Guid.NewGuid():N}@test.com"
        });

        Customer? customer = await Context.Customers.FirstAsync(c => c.Id == created.Id);
        return customer;
    }

    protected async Task<RaceEdition> SeedEditionAsync(string raceName = "Test Race", int year = 2026)
    {
        string uniqueRaceName = $"{raceName} {Guid.NewGuid():N}";
        RaceDto race = await RaceService.CreateAsync(new CreateRaceDto
        {
            Name = uniqueRaceName,
            City = "Test City",
            Country = "Test Country",
            RaceType = RaceTypeEnum.Marathon
        });

        RaceEditionDto edition = await RaceService.CreateEditionAsync(race.Id, new CreateRaceEditionDto
        {
            Year = year,
            RaceDate = new DateTime(year, 10, 12, 0, 0, 0, DateTimeKind.Utc)
        });

        RaceEdition? result = await Context.RaceEditions.FirstAsync(e => e.Id == edition.Id);
        return result;
    }

    protected async Task<RaceSlot> SeedSlotAsync(Guid editionId, string? internalCode = null, SlotStatusEnum status = SlotStatusEnum.Available, decimal acquisitionCost = 300m)
    {
        CreateRaceSlotDto dto = new()
        {
            RaceEditionId = editionId,
            InternalCode = internalCode ?? $"SLOT-{Guid.NewGuid():N}"[..20],
            Status = status,
            AcquisitionCost = acquisitionCost
        };

        RaceSlotDto created = await RaceSlotService.CreateAsync(dto);
        RaceSlot? slot = await Context.RaceSlots.FirstAsync(s => s.Id == created.Id);
        return slot;
    }

    protected async Task<Sale> SeedConfirmedSaleAsync(Customer customer, RaceEdition edition, RaceSlot slot, decimal unitPrice = 1200m)
    {
        SaleDto sale = await SaleService.CreateAsync(new CreateSaleDto
        {
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            Items =
            [
                new CreateSaleItemDto
                {
                    Description = "Marathon entry",
                    ItemType = SaleItemTypeEnum.Entry,
                    Quantity = 1,
                    UnitPrice = unitPrice
                }
            ]
        });

        SaleDto confirmed = await SaleService.ConfirmAsync(sale.Id, new ConfirmSaleDto
        {
            Slots = [new ConfirmSlotPriceDto { SlotId = slot.Id, UnitPrice = unitPrice }]
        });

        Sale? result = await Context.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .ThenInclude(p => p.Fees)
            .FirstAsync(s => s.Id == confirmed.Id);

        return result;
    }

    protected async Task<Hotel> SeedHotelAsync(string name = "Test Hotel")
    {
        string uniqueName = $"{name} {Guid.NewGuid():N}";
        HotelDto created = await HotelService.CreateAsync(new CreateHotelDto
        {
            Name = uniqueName,
            City = "Test City",
            Country = "Test Country"
        });

        Hotel? hotel = await Context.Hotels.FirstAsync(h => h.Id == created.Id);
        return hotel;
    }

    protected async Task<RunnerRegistration> SeedRegistrationAsync(Customer customer, RaceEdition edition, RaceSlot slot)
    {
        RegistrationDto created = await RegistrationService.CreateAsync(new CreateRegistrationDto
        {
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            RaceSlotId = slot.Id
        });

        RunnerRegistration? registration = await Context.RunnerRegistrations.FirstAsync(r => r.Id == created.Id);
        return registration;
    }

    protected async Task<Sale> SeedSaleWithSlotAsync(Customer customer, RaceEdition edition, RaceSlot slot, decimal unitPrice = 1200m)
    {
        SaleDto sale = await SaleService.CreateAsync(new CreateSaleDto
        {
            CustomerId = customer.Id,
            RaceEditionId = edition.Id,
            Items =
            [
                new CreateSaleItemDto
                {
                    Description = "Marathon entry",
                    ItemType = SaleItemTypeEnum.Entry,
                    Quantity = 1,
                    UnitPrice = unitPrice
                }
            ]
        });

        SaleDto confirmed = await SaleService.ConfirmAsync(sale.Id, new ConfirmSaleDto
        {
            Slots = [new ConfirmSlotPriceDto { SlotId = slot.Id, UnitPrice = unitPrice }]
        });

        Sale? result = await Context.Sales
            .Include(s => s.Items)
            .FirstAsync(s => s.Id == confirmed.Id);

        return result;
    }
}
