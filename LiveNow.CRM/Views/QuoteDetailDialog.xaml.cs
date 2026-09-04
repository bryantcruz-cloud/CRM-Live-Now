using LiveNow.CRM.Core.DTOs;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class QuoteDetailDialog : ContentDialog
{
    public QuoteDetailDialog(QuoteDto quote)
    {
        this.InitializeComponent();
        Title = $"Cotización #{quote.QuoteNumber}";

        QuoteNumberText.Text = quote.QuoteNumber;
        StatusText.Text = quote.Status.ToString();
        CurrencyText.Text = quote.Currency.ToString();
        ValidUntilText.Text = quote.ValidUntil?.ToString("dd/MM/yyyy") ?? "Sin vencimiento";
        NotesText.Text = string.IsNullOrWhiteSpace(quote.Notes) ? "Sin notas" : quote.Notes;

        ItemsListView.ItemsSource = quote.Items;

        SubtotalText.Text = quote.Subtotal.ToString("N2");
        DiscountText.Text = quote.Discount.ToString("N2");
        TaxesText.Text = quote.Taxes.ToString("N2");
        TotalText.Text = quote.Total.ToString("N2");
    }
}
