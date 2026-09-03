using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class HotelReservation : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid SaleId { get; set; }
    public Guid HotelId { get; set; }
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

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Sale Sale { get; set; } = null!;
    public Hotel Hotel { get; set; } = null!;
}
