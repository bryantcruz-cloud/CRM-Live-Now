using LiveNow.CRM.Core.Common;

namespace LiveNow.CRM.Core.Entities;

public class Cancellation : BaseEntity
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

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Sale? Sale { get; set; }
    public RaceSlot? RaceSlot { get; set; }
}
