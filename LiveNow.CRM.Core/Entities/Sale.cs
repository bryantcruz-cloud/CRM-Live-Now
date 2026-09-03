using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid RaceEditionId { get; set; }
    public Guid? QuoteId { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public DateTime SaleDate { get; set; } = DateTime.UtcNow;
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

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public RaceEdition RaceEdition { get; set; } = null!;
    public Quote? Quote { get; set; }
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<HotelReservation> HotelReservations { get; set; } = new List<HotelReservation>();
    public ICollection<CustomerChecklist> Checklists { get; set; } = new List<CustomerChecklist>();
    public ICollection<Cancellation> Cancellations { get; set; } = new List<Cancellation>();
}
