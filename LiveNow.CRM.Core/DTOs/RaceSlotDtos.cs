using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class RaceSlotDto
{
    public Guid Id { get; set; }
    public Guid RaceEditionId { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public SlotStatusEnum Status { get; set; } = SlotStatusEnum.Available;
    public decimal AcquisitionCost { get; set; }
    public CurrencyEnum AcquisitionCurrency { get; set; } = CurrencyEnum.USD;
    public Guid? SupplierId { get; set; }
    public Guid? AssignedCustomerId { get; set; }
    public DateTime? ReservationDate { get; set; }
    public DateTime? SaleDate { get; set; }
    public string? Notes { get; set; }
    public int Version { get; set; }
    public bool IsActive { get; set; } = true;

    public static RaceSlotDto FromEntity(RaceSlot entity)
    {
        return new RaceSlotDto
        {
            Id = entity.Id,
            RaceEditionId = entity.RaceEditionId,
            InternalCode = entity.InternalCode,
            Status = entity.Status,
            AcquisitionCost = entity.AcquisitionCost,
            AcquisitionCurrency = entity.AcquisitionCurrency,
            SupplierId = entity.SupplierId,
            AssignedCustomerId = entity.AssignedCustomerId,
            ReservationDate = entity.ReservationDate,
            SaleDate = entity.SaleDate,
            Notes = entity.Notes,
            Version = entity.Version,
            IsActive = entity.IsActive
        };
    }
}

public class CreateRaceSlotDto
{
    public Guid RaceEditionId { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public SlotStatusEnum Status { get; set; } = SlotStatusEnum.Available;
    public decimal AcquisitionCost { get; set; }
    public CurrencyEnum AcquisitionCurrency { get; set; } = CurrencyEnum.USD;
    public Guid? SupplierId { get; set; }
    public Guid? AssignedCustomerId { get; set; }
    public DateTime? ReservationDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRaceSlotDto
{
    public SlotStatusEnum Status { get; set; } = SlotStatusEnum.Available;
    public decimal AcquisitionCost { get; set; }
    public CurrencyEnum AcquisitionCurrency { get; set; } = CurrencyEnum.USD;
    public Guid? SupplierId { get; set; }
    public Guid? AssignedCustomerId { get; set; }
    public DateTime? ReservationDate { get; set; }
    public string? Notes { get; set; }
}

public class InventorySummaryDto
{
    public Guid RaceEditionId { get; set; }
    public int Total { get; set; }
    public int Available { get; set; }
    public int AvailableCount { get; set; }
    public int Reserved { get; set; }
    public int ReservedCount { get; set; }
    public int Sold { get; set; }
    public int SoldCount { get; set; }
    public int Registered { get; set; }
    public int RegisteredCount { get; set; }
    public int Cancelled { get; set; }
    public int Injured { get; set; }
    public int Transferable { get; set; }
    public int Transferred { get; set; }
    public int Lost { get; set; }
}