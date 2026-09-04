using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the Sales module.
/// Handles listing, searching, pagination, create, edit, and financial summary.
/// </summary>
public class SalesViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    private ObservableCollection<SaleDto> _sales = new();
    private ObservableCollection<CustomerDto> _customers = new();
    private ObservableCollection<RaceDto> _races = new();
    private ObservableCollection<RaceEditionDto> _editions = new();

    private string _searchText = string.Empty;
    private SaleStatusEnum? _selectedStatusFilter;
    private SaleDto? _selectedSale;
    private SaleFinancialSummaryDto? _financialSummary;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private int _totalPages;

    public SalesViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    #region Properties

    public ObservableCollection<SaleDto> Sales
    {
        get => _sales;
        private set => SetProperty(ref _sales, value);
    }

    public ObservableCollection<CustomerDto> Customers
    {
        get => _customers;
        private set => SetProperty(ref _customers, value);
    }

    public ObservableCollection<RaceDto> Races
    {
        get => _races;
        private set => SetProperty(ref _races, value);
    }

    public ObservableCollection<RaceEditionDto> Editions
    {
        get => _editions;
        private set => SetProperty(ref _editions, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public SaleStatusEnum? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                _ = LoadSalesAsync();
            }
        }
    }

    public SaleDto? SelectedSale
    {
        get => _selectedSale;
        set => SetProperty(ref _selectedSale, value);
    }

    public SaleFinancialSummaryDto? FinancialSummary
    {
        get => _financialSummary;
        private set => SetProperty(ref _financialSummary, value);
    }

    public int CurrentPage
    {
        get => _currentPage;
        private set => SetProperty(ref _currentPage, value);
    }

    public int PageSize
    {
        get => _pageSize;
        private set => SetProperty(ref _pageSize, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        private set => SetProperty(ref _totalPages, value);
    }

    public bool HasSales => Sales.Count > 0;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string PageInfo => TotalCount == 0
        ? "Sin resultados"
        : $"Página {CurrentPage} de {TotalPages} ({TotalCount} ventas)";

    public IReadOnlyList<SaleStatusEnum> StatusFilterOptions { get; } = new List<SaleStatusEnum>
    {
        SaleStatusEnum.Pending,
        SaleStatusEnum.Confirmed,
        SaleStatusEnum.PartiallyPaid,
        SaleStatusEnum.Paid,
        SaleStatusEnum.Cancelled,
        SaleStatusEnum.Refunded
    };

    public async Task LoadSalesAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            var result = await _apiClient.GetSalesAsync(CurrentPage, PageSize, SelectedStatusFilter);

            if (result is not null)
            {
                var filtered = FilterSales(result.Items);
                Sales = new ObservableCollection<SaleDto>(filtered);
                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
            }
            else
            {
                Sales = new ObservableCollection<SaleDto>();
                TotalCount = 0;
                TotalPages = 0;
            }

            OnPropertyChanged(nameof(HasSales));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar ventas: {ex.Message}");
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

    private IEnumerable<SaleDto> FilterSales(IEnumerable<SaleDto> sales)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return sales;
        }

        string search = SearchText.Trim().ToLowerInvariant();
        return sales.Where(s => s.SaleNumber.ToLowerInvariant().Contains(search));
    }

    public async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadSalesAsync();
    }

    public async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        await LoadSalesAsync();
    }

    public async Task GoToFirstPageAsync() => await GoToPageAsync(1);
    public async Task GoToPreviousPageAsync() => await GoToPageAsync(CurrentPage - 1);
    public async Task GoToNextPageAsync() => await GoToPageAsync(CurrentPage + 1);
    public async Task GoToLastPageAsync() => await GoToPageAsync(TotalPages);

    public async Task LoadLookupDataAsync()
    {
        try
        {
            var customersTask = _apiClient.GetCustomersAsync(page: 1, pageSize: 100);
            var racesTask = _apiClient.GetRacesAsync();

            await Task.WhenAll(customersTask, racesTask);

            var customers = await customersTask;
            var races = await racesTask;

            if (customers is not null)
            {
                Customers = new ObservableCollection<CustomerDto>(customers.Items);
            }

            if (races is not null)
            {
                Races = new ObservableCollection<RaceDto>(races);
            }
        }
        catch (Exception)
        {
            // Lookup data loading is non-critical
        }
    }

    public async Task LoadEditionsForRaceAsync(Guid raceId)
    {
        try
        {
            var editions = await _apiClient.GetEditionsAsync(raceId);
            if (editions is not null)
            {
                Editions = new ObservableCollection<RaceEditionDto>(editions);
            }
            else
            {
                Editions = new ObservableCollection<RaceEditionDto>();
            }
        }
        catch (Exception)
        {
            Editions = new ObservableCollection<RaceEditionDto>();
        }
    }

    public async Task<SaleDto?> GetSaleByIdAsync(Guid id)
    {
        ClearError();
        try
        {
            return await _apiClient.GetSaleAsync(id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al obtener venta: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
    }

    public async Task LoadFinancialSummaryAsync(Guid saleId)
    {
        try
        {
            FinancialSummary = await _apiClient.GetSaleFinancialSummaryAsync(saleId);
        }
        catch (Exception)
        {
            FinancialSummary = null;
        }
    }

    public async Task<SaleDto?> CreateSaleAsync(CreateSaleDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            var created = await _apiClient.CreateSaleAsync(dto);
            return created;
        }
        catch (ApiException ex)
        {
            SetError($"Error al crear venta: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<SaleDto?> UpdateSaleAsync(Guid id, UpdateSaleDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            var updated = await _apiClient.UpdateSaleAsync(id, dto);
            return updated;
        }
        catch (ApiException ex)
        {
            SetError($"Error al actualizar venta: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
