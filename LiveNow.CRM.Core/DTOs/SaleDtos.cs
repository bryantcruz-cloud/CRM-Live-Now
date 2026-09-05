using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class SaleDto
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid RaceEditionId { get; set; }
    public string RaceEditionName { get; set; } = string.Empty;
    public Guid? QuoteId { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public DateTime SaleDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Taxes { get; set; }
    public decimal TotalSalePrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPaymentFees { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public SaleStatusEnum Status { get; set; } = SaleStatusEnum.Pending;
    public string? Notes { get; set; }
    public IReadOnlyList<SaleItemDto> Items { get; set; } = new List<SaleItemDto>();

    public static SaleDto FromEntity(Sale entity, IReadOnlyList<SaleItem> items)
    {
        return new SaleDto
        {
            Id = entity.Id,
            SaleNumber = entity.SaleNumber,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer is null ? string.Empty : $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim(),
            RaceEditionId = entity.RaceEditionId,
            RaceEditionName = entity.RaceEdition is null ? string.Empty : entity.RaceEdition.Race is null ? entity.RaceEdition.Year.ToString() : $"{entity.RaceEdition.Race.Name} {entity.RaceEdition.Year}",
            QuoteId = entity.QuoteId,
            Currency = entity.Currency,
            SaleDate = entity.SaleDate,
            Subtotal = entity.Subtotal,
            Discount = entity.Discount,
            Taxes = entity.Taxes,
            TotalSalePrice = entity.TotalSalePrice,
            TotalCost = entity.TotalCost,
            TotalPaymentFees = entity.TotalPaymentFees,
            GrossProfit = entity.GrossProfit,
            ProfitMargin = entity.ProfitMargin,
            Status = entity.Status,
            Notes = entity.Notes,
            Items = items.Select(SaleItemDto.FromEntity).ToList()
        };
    }
}

public class SaleItemDto
{
    public Guid Id { get; set; }
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

    public static SaleItemDto FromEntity(SaleItem entity)
    {
        return new SaleItemDto
        {
            Id = entity.Id,
            SaleId = entity.SaleId,
            RaceSlotId = entity.RaceSlotId,
            Description = entity.Description,
            ItemType = entity.ItemType,
            Quantity = entity.Quantity,
            UnitCost = entity.UnitCost,
            UnitPrice = entity.UnitPrice,
            TotalCost = entity.TotalCost,
            TotalPrice = entity.TotalPrice
            ,HotelId = entity.HotelId
            ,SupplierId = entity.SupplierId
            ,CheckIn = entity.CheckIn
            ,CheckOut = entity.CheckOut
            ,Nights = entity.Nights
            ,RoomType = entity.RoomType
            ,NumberOfRooms = entity.NumberOfRooms
            ,Occupancy = entity.Occupancy
            ,BoardBasis = entity.BoardBasis
            ,ReservationPolicy = entity.ReservationPolicy
        };
    }
}

public class CreateSaleDto
{
    public Guid CustomerId { get; set; }
    public Guid RaceEditionId { get; set; }
    public Guid? QuoteId { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public decimal Discount { get; set; }
    public decimal Taxes { get; set; }
    public string? Notes { get; set; }
    public List<CreateSaleItemDto> Items { get; set; } = new();
}

public class CreateSaleItemDto
{
    public string Description { get; set; } = string.Empty;
    public SaleItemTypeEnum ItemType { get; set; } = SaleItemTypeEnum.Entry;
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateSaleDto
{
    public decimal Discount { get; set; }
    public decimal Taxes { get; set; }
    public string? Notes { get; set; }
}

public class ConfirmSlotPriceDto
{
    public Guid SlotId { get; set; }
    public decimal UnitPrice { get; set; }
}

public class ConfirmSaleDto
{
    public List<ConfirmSlotPriceDto> Slots { get; set; } = new();
}
