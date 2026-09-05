using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public sealed class PaymentsViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private SaleDto? _selectedSale;
    private SaleFinancialSummaryDto? _financialSummary;

    public PaymentsViewModel(ApiClient apiClient) => _apiClient = apiClient;
    public ObservableCollection<SaleDto> Sales { get; } = new();
    public ObservableCollection<PaymentDto> Payments { get; } = new();
    public SaleDto? SelectedSale { get => _selectedSale; private set => SetProperty(ref _selectedSale, value); }
    public SaleFinancialSummaryDto? FinancialSummary { get => _financialSummary; private set => SetProperty(ref _financialSummary, value); }
    public IReadOnlyList<PaymentMethodEnum> PaymentMethods { get; } = Enum.GetValues<PaymentMethodEnum>();
    public IReadOnlyList<PaymentFeeTypeEnum> FeeTypes { get; } = Enum.GetValues<PaymentFeeTypeEnum>();

    public async Task LoadAsync()
    {
        ClearError(); IsLoading = true;
        try
        {
            Sales.Clear();
            var result = await _apiClient.GetSalesForPaymentsAsync();
            if (result is not null) foreach (SaleDto sale in result.Items) Sales.Add(sale);
            if (SelectedSale is null && Sales.Count > 0) await SelectSaleAsync(Sales[0]);
        }
        catch (Exception ex) { SetError($"Error al cargar ventas: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    public async Task SelectSaleAsync(SaleDto? sale)
    {
        SelectedSale = sale; Payments.Clear(); FinancialSummary = null;
        if (sale is null) return;
        try
        {
            var payments = await _apiClient.GetPaymentsAsync(sale.Id);
            if (payments is not null) foreach (PaymentDto payment in payments) Payments.Add(payment);
            FinancialSummary = await _apiClient.GetFinancialSummaryAsync(sale.Id);
        }
        catch (Exception ex) { SetError($"Error al cargar el historial: {ex.Message}"); }
    }

    public async Task<bool> RegisterPaymentAsync(CreatePaymentDto dto)
    {
        if (SelectedSale is null) return false;
        ClearError(); IsLoading = true;
        try
        {
            await _apiClient.CreatePaymentAsync(SelectedSale.Id, dto);
            await LoadAsync();
            SaleDto? refreshed = Sales.FirstOrDefault(s => s.Id == SelectedSale.Id);
            await SelectSaleAsync(refreshed);
            return true;
        }
        catch (ApiException ex) { SetError($"No se pudo registrar el pago: {ex.Message}"); return false; }
        catch (Exception ex) { SetError($"Error al registrar el pago: {ex.Message}"); return false; }
        finally { IsLoading = false; }
    }
}

