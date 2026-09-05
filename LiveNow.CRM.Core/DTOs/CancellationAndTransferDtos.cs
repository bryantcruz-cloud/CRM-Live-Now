using LiveNow.CRM.Core.Entities;

namespace LiveNow.CRM.Core.DTOs;

public class CancellationDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? RaceSlotId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string SaleNumber { get; set; } = string.Empty;
    public string SlotCode { get; set; } = string.Empty;
    public string PreviousSlotStatus { get; set; } = string.Empty;
    public string NewSlotStatus { get; set; } = string.Empty;
    public DateTime CancellationDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsInjury { get; set; }
    public bool CanReassignSlot { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? Notes { get; set; }

    public static CancellationDto FromEntity(Cancellation entity)
    {
        return new CancellationDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            SaleId = entity.SaleId,
            RaceSlotId = entity.RaceSlotId,
            CustomerName = entity.Customer is null ? string.Empty : $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim(),
            SaleNumber = entity.Sale?.SaleNumber ?? string.Empty,
            SlotCode = entity.RaceSlot?.InternalCode ?? string.Empty,
            PreviousSlotStatus = entity.RaceSlotId.HasValue ? "Estado anterior registrado en auditoría" : string.Empty,
            NewSlotStatus = entity.CanReassignSlot ? "Transferable" : "Cancelled",
            CancellationDate = entity.CancellationDate,
            Reason = entity.Reason,
            IsInjury = entity.IsInjury,
            CanReassignSlot = entity.CanReassignSlot,
            RefundAmount = entity.RefundAmount,
            Notes = entity.Notes
        };
    }
}

public class CreateCancellationDto
{
    public Guid CustomerId { get; set; }
    public Guid? SaleId { get; set; }
    public Guid? RaceSlotId { get; set; }
    public DateTime CancellationDate { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public bool IsInjury { get; set; }
    public bool CanReassignSlot { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? Notes { get; set; }
}

public class SlotTransferDto
{
    public Guid Id { get; set; }
    public Guid RaceSlotId { get; set; }
    public Guid? FromCustomerId { get; set; }
    public Guid? ToCustomerId { get; set; }
    public string FromCustomerName { get; set; } = string.Empty;
    public string ToCustomerName { get; set; } = string.Empty;
    public string SlotCode { get; set; } = string.Empty;
    public string PreviousSlotStatus { get; set; } = string.Empty;
    public string NewSlotStatus { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public static SlotTransferDto FromEntity(SlotTransfer entity)
    {
        return new SlotTransferDto
        {
            Id = entity.Id,
            RaceSlotId = entity.RaceSlotId,
            FromCustomerId = entity.FromCustomerId,
            ToCustomerId = entity.ToCustomerId,
            FromCustomerName = entity.FromCustomerId.HasValue ? entity.RaceSlot?.AssignedCustomer?.FirstName ?? string.Empty : string.Empty,
            ToCustomerName = entity.ToCustomerId.HasValue ? entity.RaceSlot?.AssignedCustomer?.FirstName ?? string.Empty : string.Empty,
            SlotCode = entity.RaceSlot?.InternalCode ?? string.Empty,
            PreviousSlotStatus = "Transferable/Reserved",
            NewSlotStatus = "Reserved",
            TransferDate = entity.TransferDate,
            Reason = entity.Reason,
            Notes = entity.Notes
        };
    }
}

public class CreateSlotTransferDto
{
    public Guid TransferSlotId { get; set; }
    public Guid ToCustomerId { get; set; }
    public Guid? FromCustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
}

public class SlotTransferRequestDto
{
    public Guid ToCustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
