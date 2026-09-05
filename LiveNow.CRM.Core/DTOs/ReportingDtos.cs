namespace LiveNow.CRM.Core.DTOs;

using LiveNow.CRM.Core.Enums;

public class SaleFinancialSummaryDto
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public decimal SaleTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal TotalCost { get; set; }
    public decimal PaymentFees { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public string CurrencyCode { get; set; } = "USD";
}

public class FinancialComputationDto
{
    public decimal GrossProfit { get; set; }
    public decimal ProfitMargin { get; set; }

    public static FinancialComputationDto Compute(
        decimal totalSalePrice,
        decimal totalCost,
        decimal totalPaymentFees)
    {
        decimal grossProfit = totalSalePrice - totalCost - totalPaymentFees;
        decimal profitMargin = totalSalePrice == 0m ? 0m : grossProfit / totalSalePrice * 100m;
        return new FinancialComputationDto
        {
            GrossProfit = grossProfit,
            ProfitMargin = profitMargin
        };
    }
}

public class AuditLogDto
{
    public Guid Id { get; set; }
    public string? UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
}

public class OperationalSummaryDto
{
    public int TotalCustomers { get; set; }
    public int TotalSales { get; set; }
    public int PendingPayments { get; set; }
    public int PendingRegistrations { get; set; }
    public int AvailableSlots { get; set; }
    public int ReservedSlots { get; set; }
    public int SoldSlots { get; set; }
    public int TransferableSlots { get; set; }
    public int TotalCancellations { get; set; }
    public int InjuredCustomers { get; set; }
    public int Hotels { get; set; }
    public int HotelReservations { get; set; }
    public decimal SalesTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal GrossProfit { get; set; }
}
