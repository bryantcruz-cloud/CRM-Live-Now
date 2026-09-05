using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class HotelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public static HotelDto FromEntity(Hotel entity)
    {
        return new HotelDto
        {
            Id = entity.Id,
            Name = entity.Name,
            City = entity.City,
            Country = entity.Country,
            SupplierId = entity.SupplierId,
            SupplierName = entity.Supplier?.Name ?? string.Empty,
            Notes = entity.Notes,
            IsActive = entity.IsActive
        };
    }
}

public class CreateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateHotelDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class HotelReservationDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid HotelId { get; set; }
    public string HotelName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public int Nights { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int Occupancy { get; set; }
    public int NumberOfRooms { get; set; }
    public decimal Cost { get; set; }
    public decimal SalePrice { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public HotelReservationStatusEnum Status { get; set; } = HotelReservationStatusEnum.Pending;
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }

    public static HotelReservationDto FromEntity(HotelReservation entity)
    {
        return new HotelReservationDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer is null ? string.Empty : $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim(),
            SaleId = entity.SaleId,
            SaleNumber = entity.Sale?.SaleNumber ?? string.Empty,
            HotelId = entity.HotelId,
            HotelName = entity.Hotel?.Name ?? string.Empty,
            SupplierName = entity.Hotel?.Supplier?.Name ?? string.Empty,
            CheckIn = entity.CheckIn,
            CheckOut = entity.CheckOut,
            Nights = entity.Nights,
            RoomType = entity.RoomType,
            Occupancy = entity.Occupancy,
            NumberOfRooms = entity.NumberOfRooms,
            Cost = entity.Cost,
            SalePrice = entity.SalePrice,
            Currency = entity.Currency,
            Status = entity.Status,
            ConfirmationNumber = entity.ConfirmationNumber,
            Notes = entity.Notes
        };
    }
}

public class CreateHotelReservationDto
{
    public Guid CustomerId { get; set; }
    public Guid SaleId { get; set; }
    public Guid HotelId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int Occupancy { get; set; }
    public int NumberOfRooms { get; set; }
    public decimal Cost { get; set; }
    public decimal SalePrice { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdateHotelReservationDto
{
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public string RoomType { get; set; } = string.Empty;
    public int Occupancy { get; set; }
    public int NumberOfRooms { get; set; }
    public decimal Cost { get; set; }
    public decimal SalePrice { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public HotelReservationStatusEnum Status { get; set; } = HotelReservationStatusEnum.Pending;
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }
}
