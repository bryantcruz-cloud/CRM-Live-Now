using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;

namespace LiveNow.CRM.API.Services;

public class FinancialCalculatorService : IFinancialCalculator
{
    public FinancialComputationDto Compute(
        decimal totalSalePrice,
        decimal totalCost,
        decimal totalPaymentFees)
    {
        return FinancialComputationDto.Compute(totalSalePrice, totalCost, totalPaymentFees);
    }
}