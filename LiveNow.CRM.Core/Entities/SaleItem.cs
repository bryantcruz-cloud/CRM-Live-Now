using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid? RaceSlotId { get; set; }
    public string Description { get; set; } = string.Empty;
    public SaleItemTypeEnum ItemType { get; set; } = SaleItemTypeEnum.Entry;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid? HotelId { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public int? Nights { get; set; }
    public string? RoomType { get; set; }
    public int? NumberOfRooms { get; set; }
    public int? Occupancy { get; set; }
    public HotelBoardBasisEnum? BoardBasis { get; set; }
    public string? ReservationPolicy { get; set; }

    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public RaceSlot? RaceSlot { get; set; }
}
