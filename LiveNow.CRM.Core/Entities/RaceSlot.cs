using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class RaceSlot : BaseEntity
{
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

    // Navigation properties
    public RaceEdition RaceEdition { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public Customer? AssignedCustomer { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<RunnerRegistration> RunnerRegistrations { get; set; } = new List<RunnerRegistration>();
    public ICollection<Cancellation> Cancellations { get; set; } = new List<Cancellation>();
    public ICollection<SlotTransfer> SlotTransfers { get; set; } = new List<SlotTransfer>();
}
