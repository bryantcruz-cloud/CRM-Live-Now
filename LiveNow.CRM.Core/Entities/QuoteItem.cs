using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class QuoteItem : BaseEntity
{
    public Guid QuoteId { get; set; }
    public string Description { get; set; } = string.Empty;
    public QuoteItemTypeEnum ItemType { get; set; } = QuoteItemTypeEnum.Entry;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Quote Quote { get; set; } = null!;
}
