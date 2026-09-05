using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the Dashboard page.
/// Loads summary cards: customers, quotes, sales, pending payments, available slots.
/// </summary>
public class DashboardViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    private int _totalCustomers;
    private int _totalQuotes;
    private int _totalSales;
    private int _pendingPayments;
    private int _availableSlots;
    private string _lastUpdated = string.Empty;

    public DashboardViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public int TotalCustomers
    {
        get => _totalCustomers;
        set => SetProperty(ref _totalCustomers, value);
    }

    public int TotalQuotes
    {
        get => _totalQuotes;
        set => SetProperty(ref _totalQuotes, value);
    }

    public int TotalSales
    {
        get => _totalSales;
        set => SetProperty(ref _totalSales, value);
    }

    public int PendingPayments
    {
        get => _pendingPayments;
        set => SetProperty(ref _pendingPayments, value);
    }

    public int AvailableSlots
    {
        get => _availableSlots;
        set => SetProperty(ref _availableSlots, value);
    }

    public string LastUpdated
    {
        get => _lastUpdated;
        set => SetProperty(ref _lastUpdated, value);
    }

    public async Task LoadDashboardDataAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            // Load customers count
            PagedResult<CustomerDto>? customers = await _apiClient.GetCustomersAsync(page: 1, pageSize: 1);
            TotalCustomers = customers?.TotalCount ?? 0;

            // Load quotes count
            PagedResult<QuoteDto>? quotes = await _apiClient.GetQuotesAsync(page: 1, pageSize: 1);
            TotalQuotes = quotes?.TotalCount ?? 0;

            // Load sales count
            PagedResult<SaleDto>? sales = await _apiClient.GetSalesAsync(page: 1, pageSize: 1);
            TotalSales = sales?.TotalCount ?? 0;

            OperationalSummaryDto? summary = await _apiClient.GetOperationalSummaryAsync();
            if (summary is not null)
            {
                TotalCustomers = summary.TotalCustomers;
                TotalSales = summary.TotalSales;
                PendingPayments = summary.PendingPayments;
                AvailableSlots = summary.AvailableSlots;
            }

            LastUpdated = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }
        catch (ApiException ex)
        {
            SetError($"Error al conectar con la API: {ex.Message}");
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
