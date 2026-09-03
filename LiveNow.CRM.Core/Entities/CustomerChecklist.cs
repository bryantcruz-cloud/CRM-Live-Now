using LiveNow.CRM.Core.Common;

namespace LiveNow.CRM.Core.Entities;

public class CustomerChecklist : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid SaleId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public Sale Sale { get; set; } = null!;
}
