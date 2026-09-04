using LiveNow.CRM.Core.DTOs;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class SaleDetailDialog : ContentDialog
{
    public SaleDetailDialog(SaleDto sale, SaleFinancialSummaryDto? financialSummary)
    {
        this.InitializeComponent();
        Title = $"Venta #{sale.SaleNumber}";

        SaleNumberText.Text = sale.SaleNumber;
        StatusText.Text = sale.Status.ToString();
        CurrencyText.Text = sale.Currency.ToString();
        SaleDateText.Text = sale.SaleDate.ToString("dd/MM/yyyy");
        NotesText.Text = string.IsNullOrWhiteSpace(sale.Notes) ? "Sin notas" : sale.Notes;

        ItemsListView.ItemsSource = sale.Items;

        SubtotalText.Text = sale.Subtotal.ToString("N2");
        DiscountText.Text = sale.Discount.ToString("N2");
        TaxesText.Text = sale.Taxes.ToString("N2");
        TotalText.Text = sale.TotalSalePrice.ToString("N2");

        if (financialSummary != null)
        {
            TotalPaidText.Text = financialSummary.TotalPaid.ToString("N2");
            OutstandingBalanceText.Text = financialSummary.OutstandingBalance.ToString("N2");
            GrossProfitText.Text = financialSummary.GrossProfit.ToString("N2");
            ProfitMarginText.Text = (financialSummary.ProfitMargin * 100).ToString("N2");
        }
        else
        {
            TotalPaidText.Text = "N/A";
            OutstandingBalanceText.Text = "N/A";
            GrossProfitText.Text = "N/A";
            ProfitMarginText.Text = "N/A";
        }
    }
}
