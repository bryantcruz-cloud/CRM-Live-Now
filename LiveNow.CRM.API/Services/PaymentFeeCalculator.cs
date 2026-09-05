using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;

namespace LiveNow.CRM.API.Services;

public class PaymentFeeCalculator : IPaymentFeeCalculator
{
    public decimal Calculate(
        decimal amount,
        PaymentFeeTypeEnum feeType,
        decimal? rate,
        decimal? fixedAmount)
    {
        if (amount < 0m)
        {
            throw new ValidationException("PAYMENT_AMOUNT_INVALID", "El monto no puede ser negativo.");
        }

        return feeType switch
        {
            PaymentFeeTypeEnum.NoFee => 0m,
            PaymentFeeTypeEnum.Percentage => amount * RequireRate(feeType, rate) / 100m,
            PaymentFeeTypeEnum.FixedAmount => RequireFixedAmount(feeType, fixedAmount),
            PaymentFeeTypeEnum.PercentagePlusFixed => (amount * RequireRate(feeType, rate) / 100m) + RequireFixedAmount(feeType, fixedAmount),
            _ => throw new ValidationException("PAYMENT_FEE_TYPE_INVALID", "Tipo de comisión no válido.")
        };
    }

    private static decimal RequireRate(PaymentFeeTypeEnum feeType, decimal? rate)
    {
        if (rate is null)
        {
            throw new ValidationException("PAYMENT_FEE_RATE_REQUIRED", $"Se requiere una tasa para el tipo {feeType}.");
        }

        if (rate < 0m)
        {
            throw new ValidationException("PAYMENT_FEE_RATE_INVALID", "La tasa no puede ser negativa.");
        }

        return rate.Value;
    }

    private static decimal RequireFixedAmount(PaymentFeeTypeEnum feeType, decimal? fixedAmount)
    {
        if (fixedAmount is null)
        {
            throw new ValidationException("PAYMENT_FEE_FIXED_REQUIRED", $"Se requiere un monto fijo para el tipo {feeType}.");
        }

        if (fixedAmount < 0m)
        {
            throw new ValidationException("PAYMENT_FEE_FIXED_INVALID", "El monto fijo no puede ser negativo.");
        }

        return fixedAmount.Value;
    }
}