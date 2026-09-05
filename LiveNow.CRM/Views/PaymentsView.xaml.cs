using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LiveNow.CRM.Views;

public sealed partial class PaymentsView : Page
{
    public PaymentsViewModel ViewModel { get; private set; } = null!;
    public PaymentsView() => InitializeComponent();

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is LiveNow.CRM.Services.ApiClient apiClient)
        {
            ViewModel = new PaymentsViewModel(apiClient);
            await ViewModel.LoadAsync();
            RefreshSummary();
        }
    }

    private async void Sale_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        await ViewModel.SelectSaleAsync(ViewModel.SelectedSale);
        RefreshSummary();
    }

    private void RefreshSummary()
    {
        SaleFinancialSummaryDto? summary = ViewModel.FinancialSummary;
        SaleTotalText.Text = summary is null ? "Venta: —" : $"Venta: {summary.SaleTotal:N2} {summary.CurrencyCode}";
        PaidText.Text = summary is null ? "Cobrado: —" : $"Cobrado: {summary.PaidAmount:N2}";
        OutstandingText.Text = summary is null ? "Pendiente: —" : $"Pendiente: {summary.OutstandingBalance:N2}";
        FeesText.Text = summary is null ? "Comisiones: —" : $"Comisiones: {summary.PaymentFees:N2}";
        ProfitText.Text = summary is null ? "Ganancia: —" : $"Ganancia: {summary.GrossProfit:N2} ({summary.ProfitMargin:N2}%)";
    }

    private async void RegisterPayment_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedSale is null) return;
        TextBox amount = new() { PlaceholderText = "Monto" };
        ComboBox method = new() { ItemsSource = ViewModel.PaymentMethods, SelectedIndex = 0 };
        TextBox reference = new() { PlaceholderText = "Referencia/comprobante" };
        TextBox notes = new() { PlaceholderText = "Notas" };
        ComboBox feeType = new() { ItemsSource = ViewModel.FeeTypes, SelectedIndex = 3 };
        TextBox feeRate = new() { PlaceholderText = "Tasa % (si aplica)" };
        TextBox fixedFee = new() { PlaceholderText = "Comisión fija (si aplica)" };
        ComboBox currency = new() { ItemsSource = Enum.GetValues<CurrencyEnum>(), SelectedItem = ViewModel.SelectedSale.Currency };
        DatePicker paymentDate = new() { Date = DateTimeOffset.Now };
        StackPanel panel = new() { Spacing = 8, Children = { amount, paymentDate, currency, method, reference, notes, feeType, feeRate, fixedFee } };
        ContentDialog dialog = new() { Title = $"Pago para {ViewModel.SelectedSale.SaleNumber}", Content = panel, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary || !decimal.TryParse(amount.Text, out decimal value) || value <= 0) return;
        PaymentFeeTypeEnum selectedFee = (PaymentFeeTypeEnum)feeType.SelectedItem;
        CreatePaymentDto dto = new() { Amount = value, PaymentDate = paymentDate.Date.DateTime, Currency = (CurrencyEnum)currency.SelectedItem, PaymentMethod = (PaymentMethodEnum)method.SelectedItem, Reference = reference.Text, Notes = notes.Text, Fee = selectedFee == PaymentFeeTypeEnum.NoFee ? null : new CreatePaymentFeeDto { FeeType = selectedFee, Rate = decimal.TryParse(feeRate.Text, out decimal rate) ? rate : null, FixedAmount = decimal.TryParse(fixedFee.Text, out decimal fixedAmount) ? fixedAmount : null, Currency = (CurrencyEnum)currency.SelectedItem } };
        await ViewModel.RegisterPaymentAsync(dto);
        RefreshSummary();
    }
}
