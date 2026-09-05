using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;

namespace LiveNow.CRM.API.Services;

/// <summary>
/// Shared financial recalculation helpers.
/// All sale financial figures are ALWAYS recomputed from source data
/// (sale items, payments and payment fees) - never trusted from stored values.
/// </summary>
internal static class SaleFinancials
{
    /// <summary>
    /// Payments that count towards the customer balance:
    /// registered (Pending) or received (Completed).
    /// Failed, Refunded and Cancelled payments never count.
    /// </summary>
    public static bool CountsTowardsBalance(PaymentStatusEnum status)
    {
        return status is PaymentStatusEnum.Pending or PaymentStatusEnum.Completed;
    }

    public static decimal SumValidPayments(IEnumerable<Payment> payments)
    {
        return payments
            .Where(p => CountsTowardsBalance(p.Status))
            .Sum(p => p.Amount);
    }

    public static decimal SumPaymentFees(IEnumerable<Payment> payments)
    {
        return payments
            .Where(p => CountsTowardsBalance(p.Status))
            .SelectMany(p => p.Fees)
            .Sum(f => f.CalculatedAmount);
    }

    public static decimal SumItemsCost(IEnumerable<SaleItem> items)
    {
        return items.Sum(i => i.TotalCost);
    }

    public static decimal SumItemsPrice(IEnumerable<SaleItem> items)
    {
        return items.Sum(i => i.TotalPrice);
    }

    /// <summary>
    /// Recomputes and writes back all financial fields of the sale
    /// from its items, payments and fees, using <see cref="IFinancialCalculator"/>.
    /// </summary>
    public static void Apply(Sale sale, IFinancialCalculator calculator)
    {
        sale.TotalCost = SumItemsCost(sale.Items);
        sale.TotalPaymentFees = SumPaymentFees(sale.Payments);

        FinancialComputationDto result = calculator.Compute(sale.TotalSalePrice, sale.TotalCost, sale.TotalPaymentFees);
        sale.GrossProfit = result.GrossProfit;
        sale.ProfitMargin = result.ProfitMargin;
    }

    /// <summary>
    /// Updates the sale status according to the paid amount.
    /// </summary>
    public static void UpdateStatusFromPayments(Sale sale, decimal paidAmount)
    {
        if (sale.Status is SaleStatusEnum.Cancelled or SaleStatusEnum.Refunded)
        {
            return;
        }

        if (paidAmount >= sale.TotalSalePrice && sale.TotalSalePrice > 0m)
        {
            sale.Status = SaleStatusEnum.Paid;
        }
        else if (paidAmount > 0m)
        {
            sale.Status = SaleStatusEnum.PartiallyPaid;
        }
        else
        {
            sale.Status = SaleStatusEnum.Confirmed;
        }
    }
}
