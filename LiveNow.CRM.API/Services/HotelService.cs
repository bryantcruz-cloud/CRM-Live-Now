using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class HotelService : IHotelService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public HotelService(LiveNowDbContext context, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<HotelDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Hotel> hotels = await _context.Hotels
            .AsNoTracking()
            .Include(h => h.Supplier)
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);

        return hotels.Select(HotelDto.FromEntity).ToList();
    }

    public async Task<HotelDto> CreateAsync(CreateHotelDto dto, CancellationToken cancellationToken = default)
    {
        ValidateName(dto.Name);

        if (dto.SupplierId is not null &&
            !await _context.Suppliers.AnyAsync(s => s.Id == dto.SupplierId, cancellationToken))
        {
            throw new NotFoundException("SUPPLIER_NOT_FOUND", "El proveedor indicado no existe.");
        }

        Hotel hotel = new()
        {
            Name = dto.Name.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            SupplierId = dto.SupplierId,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Hotels.Add(hotel);
        await _auditService.RecordAsync("Hotel", hotel.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return HotelDto.FromEntity(hotel);
    }

    public async Task<HotelDto> UpdateAsync(Guid id, UpdateHotelDto dto, CancellationToken cancellationToken = default)
    {
        Hotel? hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == id, cancellationToken)
            ?? throw new NotFoundException("HOTEL_NOT_FOUND", "El hotel no existe.");

        ValidateName(dto.Name);

        hotel.Name = dto.Name.Trim();
        hotel.City = dto.City.Trim();
        hotel.Country = dto.Country.Trim();
        hotel.SupplierId = dto.SupplierId;
        hotel.Notes = dto.Notes;
        hotel.IsActive = dto.IsActive;
        hotel.UpdatedAt = DateTime.UtcNow;

        await _auditService.RecordAsync("Hotel", hotel.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return HotelDto.FromEntity(hotel);
    }

    public async Task<IReadOnlyList<HotelReservationDto>> GetReservationsAsync(CancellationToken cancellationToken = default)
    {
        List<HotelReservation> reservations = await _context.HotelReservations
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Sale)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.Supplier)
            .OrderByDescending(r => r.CheckIn)
            .ToListAsync(cancellationToken);

        return reservations.Select(HotelReservationDto.FromEntity).ToList();
    }

    public async Task<HotelReservationDto> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        HotelReservation? reservation = await _context.HotelReservations
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Sale)
            .Include(r => r.Hotel)
                .ThenInclude(h => h.Supplier)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("HOTEL_RESERVATION_NOT_FOUND", "La reserva de hotel no existe.");

        return HotelReservationDto.FromEntity(reservation);
    }

    public async Task<HotelReservationDto> CreateReservationAsync(CreateHotelReservationDto dto, CancellationToken cancellationToken = default)
    {
        await ValidateReservationReferencesAsync(dto.CustomerId, dto.SaleId, dto.HotelId, cancellationToken);
        int nights = ValidateDates(dto.CheckIn, dto.CheckOut);

        HotelReservation reservation = new()
        {
            CustomerId = dto.CustomerId,
            SaleId = dto.SaleId,
            HotelId = dto.HotelId,
            CheckIn = dto.CheckIn,
            CheckOut = dto.CheckOut,
            Nights = nights,
            RoomType = dto.RoomType.Trim(),
            Occupancy = dto.Occupancy,
            NumberOfRooms = dto.NumberOfRooms,
            Cost = dto.Cost,
            SalePrice = dto.SalePrice,
            Currency = dto.Currency,
            Status = HotelReservationStatusEnum.Pending,
            ConfirmationNumber = dto.ConfirmationNumber,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        ValidateAmounts(reservation.Cost, reservation.SalePrice);

        _context.HotelReservations.Add(reservation);
        await _auditService.RecordAsync("HotelReservation", reservation.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return HotelReservationDto.FromEntity(reservation);
    }

    public async Task<HotelReservationDto> UpdateReservationAsync(Guid id, UpdateHotelReservationDto dto, CancellationToken cancellationToken = default)
    {
        HotelReservation? reservation = await _context.HotelReservations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("HOTEL_RESERVATION_NOT_FOUND", "La reserva de hotel no existe.");

        int nights = ValidateDates(dto.CheckIn, dto.CheckOut);

        reservation.CheckIn = dto.CheckIn;
        reservation.CheckOut = dto.CheckOut;
        reservation.Nights = nights;
        reservation.RoomType = dto.RoomType.Trim();
        reservation.Occupancy = dto.Occupancy;
        reservation.NumberOfRooms = dto.NumberOfRooms;
        reservation.Cost = dto.Cost;
        reservation.SalePrice = dto.SalePrice;
        reservation.Currency = dto.Currency;
        reservation.Status = dto.Status;
        reservation.ConfirmationNumber = dto.ConfirmationNumber;
        reservation.Notes = dto.Notes;
        reservation.UpdatedAt = DateTime.UtcNow;

        ValidateAmounts(reservation.Cost, reservation.SalePrice);

        await _auditService.RecordAsync("HotelReservation", reservation.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return HotelReservationDto.FromEntity(reservation);
    }

    private async Task ValidateReservationReferencesAsync(Guid customerId, Guid saleId, Guid hotelId, CancellationToken cancellationToken)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        if (!await _context.Sales.AnyAsync(s => s.Id == saleId, cancellationToken))
        {
            throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");
        }

        if (!await _context.Hotels.AnyAsync(h => h.Id == hotelId, cancellationToken))
        {
            throw new NotFoundException("HOTEL_NOT_FOUND", "El hotel no existe.");
        }
    }

    /// <summary>CheckOut must be after CheckIn; nights are derived from the dates.</summary>
    private static int ValidateDates(DateTime checkIn, DateTime checkOut)
    {
        if (checkOut <= checkIn)
        {
            throw new ValidationException("HOTEL_DATES_INVALID", "La fecha de check-out debe ser posterior a la de check-in.");
        }

        int nights = (int)(checkOut.Date - checkIn.Date).TotalDays;

        if (nights <= 0)
        {
            throw new ValidationException("HOTEL_NIGHTS_INVALID", "La reserva debe ser de al menos una noche.");
        }

        return nights;
    }

    private static void ValidateAmounts(decimal cost, decimal salePrice)
    {
        if (cost < 0m || salePrice < 0m)
        {
            throw new ValidationException("HOTEL_AMOUNTS_INVALID", "El costo y el precio de venta no pueden ser negativos.");
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("HOTEL_NAME_REQUIRED", "El nombre del hotel es obligatorio.");
        }
    }
}
