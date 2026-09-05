using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IPaymentFeeCalculator
{
    decimal Calculate(
        decimal amount,
        PaymentFeeTypeEnum feeType,
        decimal? rate,
        decimal? fixedAmount);
}

public interface IFinancialCalculator
{
    FinancialComputationDto Compute(
        decimal totalSalePrice,
        decimal totalCost,
        decimal totalPaymentFees);
}