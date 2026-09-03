using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class Payment : BaseEntity
{
    public Guid SaleId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.Card;
    public string? Reference { get; set; }
    public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;
    public string? Notes { get; set; }

    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public ICollection<PaymentFee> Fees { get; set; } = new List<PaymentFee>();
}
