using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class Quote : BaseEntity
{
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

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public RaceEdition RaceEdition { get; set; } = null!;
    public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
