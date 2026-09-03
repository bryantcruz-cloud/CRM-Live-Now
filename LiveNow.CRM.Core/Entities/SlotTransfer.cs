using LiveNow.CRM.Core.Common;

namespace LiveNow.CRM.Core.Entities;

public class SlotTransfer : BaseEntity
{
    public Guid RaceSlotId { get; set; }
    public Guid? FromCustomerId { get; set; }
    public Guid? ToCustomerId { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Navigation properties
    public RaceSlot RaceSlot { get; set; } = null!;
}
