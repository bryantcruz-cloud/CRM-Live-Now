using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class PaymentFee : BaseEntity
{
    public Guid PaymentId { get; set; }
    public PaymentFeeTypeEnum FeeType { get; set; } = PaymentFeeTypeEnum.NoFee;
    public decimal? Rate { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal CalculatedAmount { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public string? Notes { get; set; }

    // Navigation properties
    public Payment Payment { get; set; } = null!;
}
