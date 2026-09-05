using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class QuoteDto
{
    public Guid Id { get; set; }
    public string QuoteNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid RaceEditionId { get; set; }
    public QuoteStatusEnum Status { get; set; } = QuoteStatusEnum.Draft;
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public DateTime? ValidUntil { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Taxes { get; set; }
    public decimal Total { get; set; }
    public string? Notes { get; set; }
    public IReadOnlyList<QuoteItemDto> Items { get; set; } = new List<QuoteItemDto>();

    public static QuoteDto FromEntity(Quote entity, IReadOnlyList<QuoteItem> items)
    {
        return new QuoteDto
        {
            Id = entity.Id,
            QuoteNumber = entity.QuoteNumber,
            CustomerId = entity.CustomerId,
            RaceEditionId = entity.RaceEditionId,
            Status = entity.Status,
            Currency = entity.Currency,
            ValidUntil = entity.ValidUntil,
            Subtotal = entity.Subtotal,
            Discount = entity.Discount,
            Taxes = entity.Taxes,
            Total = entity.Total,
            Notes = entity.Notes,
            Items = items.Select(QuoteItemDto.FromEntity).ToList()
        };
    }
}

public class QuoteItemDto
{
    public Guid Id { get; set; }
    public Guid QuoteId { get; set; }
    public string Description { get; set; } = string.Empty;
    public QuoteItemTypeEnum ItemType { get; set; } = QuoteItemTypeEnum.Entry;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
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

    public static QuoteItemDto FromEntity(QuoteItem entity)
    {
        return new QuoteItemDto
        {
            Id = entity.Id,
            QuoteId = entity.QuoteId,
            Description = entity.Description,
            ItemType = entity.ItemType,
            Quantity = entity.Quantity,
            UnitCost = entity.UnitCost,
            UnitPrice = entity.UnitPrice,
            TotalCost = entity.TotalCost,
            TotalPrice = entity.TotalPrice,
            Notes = entity.Notes,
            HotelId = entity.HotelId,
            SupplierId = entity.SupplierId,
            CheckIn = entity.CheckIn,
            CheckOut = entity.CheckOut,
            Nights = entity.Nights,
            RoomType = entity.RoomType,
            NumberOfRooms = entity.NumberOfRooms,
            Occupancy = entity.Occupancy,
            BoardBasis = entity.BoardBasis,
            ReservationPolicy = entity.ReservationPolicy
        };
    }
}

public class CreateQuoteDto
{
    public Guid CustomerId { get; set; }
    public Guid RaceEditionId { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public DateTime? ValidUntil { get; set; }
    public decimal Discount { get; set; }
    public decimal Taxes { get; set; }
    public string? Notes { get; set; }
    public List<CreateQuoteItemDto> Items { get; set; } = new();
}

public class CreateQuoteItemDto
{
    public string Description { get; set; } = string.Empty;
    public QuoteItemTypeEnum ItemType { get; set; } = QuoteItemTypeEnum.Entry;
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
    public Guid? HotelId { get; set; }
    public Guid? SupplierId { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string? RoomType { get; set; }
    public int? NumberOfRooms { get; set; }
    public int? Occupancy { get; set; }
    public HotelBoardBasisEnum? BoardBasis { get; set; }
    public string? ReservationPolicy { get; set; }
}

public class UpdateQuoteDto
{
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public DateTime? ValidUntil { get; set; }
    public decimal Discount { get; set; }
    public decimal Taxes { get; set; }
    public string? Notes { get; set; }
    public List<CreateQuoteItemDto> Items { get; set; } = new();
}

public class ConvertQuoteToSaleDto
{
    public List<ConvertSlotAssignmentDto> SlotAssignments { get; set; } = new();
}

public class ConvertSlotAssignmentDto
{
    public Guid SlotId { get; set; }
    public decimal UnitPrice { get; set; }
}
