using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.Card;
    public string? Reference { get; set; }
    public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Pending;
    public string? Notes { get; set; }
    public IReadOnlyList<PaymentFeeDto> Fees { get; set; } = new List<PaymentFeeDto>();

    public static PaymentDto FromEntity(Payment entity, IReadOnlyList<PaymentFee> fees)
    {
        return new PaymentDto
        {
            Id = entity.Id,
            SaleId = entity.SaleId,
            PaymentDate = entity.PaymentDate,
            Amount = entity.Amount,
            Currency = entity.Currency,
            PaymentMethod = entity.PaymentMethod,
            Reference = entity.Reference,
            Status = entity.Status,
            Notes = entity.Notes,
            Fees = fees.Select(PaymentFeeDto.FromEntity).ToList()
        };
    }
}

public class PaymentFeeDto
{
    public Guid Id { get; set; }
    public Guid PaymentId { get; set; }
    public PaymentFeeTypeEnum FeeType { get; set; } = PaymentFeeTypeEnum.NoFee;
    public decimal? Rate { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal CalculatedAmount { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public string? Notes { get; set; }

    public static PaymentFeeDto FromEntity(PaymentFee entity)
    {
        return new PaymentFeeDto
        {
            Id = entity.Id,
            PaymentId = entity.PaymentId,
            FeeType = entity.FeeType,
            Rate = entity.Rate,
            FixedAmount = entity.FixedAmount,
            CalculatedAmount = entity.CalculatedAmount,
            Currency = entity.Currency,
            Notes = entity.Notes
        };
    }
}

public class CreatePaymentDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.Card;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public CreatePaymentFeeDto? Fee { get; set; }
}

public class CreatePaymentFeeDto
{
    public PaymentFeeTypeEnum FeeType { get; set; } = PaymentFeeTypeEnum.NoFee;
    public decimal? Rate { get; set; }
    public decimal? FixedAmount { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public string? Notes { get; set; }
}

public class UpdatePaymentDto
{
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public PaymentMethodEnum PaymentMethod { get; set; } = PaymentMethodEnum.Card;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public PaymentStatusEnum Status { get; set; } = PaymentStatusEnum.Completed;
    public CreatePaymentFeeDto? Fee { get; set; }
}