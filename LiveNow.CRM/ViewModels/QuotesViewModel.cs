using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the Quotes module.
/// Handles listing, searching, pagination, create, edit, and status changes.
/// </summary>
public class QuotesViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    private ObservableCollection<QuoteDto> _quotes = new();
    private ObservableCollection<CustomerDto> _customers = new();
    private ObservableCollection<RaceDto> _races = new();
    private ObservableCollection<RaceEditionDto> _editions = new();

    private string _searchText = string.Empty;
    private QuoteStatusEnum? _selectedStatusFilter;
    private QuoteDto? _selectedQuote;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private int _totalPages;


    public QuotesViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public ObservableCollection<QuoteDto> Quotes
    {
        get => _quotes;
        private set => SetProperty(ref _quotes, value);
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

    public QuoteStatusEnum? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                _ = LoadQuotesAsync();
            }
        }
    }

    public QuoteDto? SelectedQuote
    {
        get => _selectedQuote;
        set => SetProperty(ref _selectedQuote, value);
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

    public bool HasQuotes => Quotes.Count > 0;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string PageInfo => TotalCount == 0
        ? "Sin resultados"
        : $"Página {CurrentPage} de {TotalPages} ({TotalCount} cotizaciones)";

    public IReadOnlyList<QuoteStatusEnum> StatusFilterOptions { get; } = new List<QuoteStatusEnum>
    {
        QuoteStatusEnum.Draft,
        QuoteStatusEnum.Sent,
        QuoteStatusEnum.Accepted,
        QuoteStatusEnum.Rejected,
        QuoteStatusEnum.Expired,
        QuoteStatusEnum.ConvertedToSale
    };

    public async Task LoadQuotesAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            var result = await _apiClient.GetQuotesAsync(CurrentPage, PageSize, SelectedStatusFilter);

            if (result is not null)
            {
                var filtered = FilterQuotes(result.Items);
                Quotes = new ObservableCollection<QuoteDto>(filtered);
                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
            }
            else
            {
                Quotes = new ObservableCollection<QuoteDto>();
                TotalCount = 0;
                TotalPages = 0;
            }

            OnPropertyChanged(nameof(HasQuotes));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar cotizaciones: {ex.Message}");
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

    private IEnumerable<QuoteDto> FilterQuotes(IEnumerable<QuoteDto> quotes)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return quotes;
        }

        string search = SearchText.Trim().ToLowerInvariant();
        return quotes.Where(q => q.QuoteNumber.ToLowerInvariant().Contains(search));
    }

    public async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadQuotesAsync();
    }

    public async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        await LoadQuotesAsync();
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

    public async Task<QuoteDto?> GetQuoteByIdAsync(Guid id)
    {
        ClearError();
        try
        {
            return await _apiClient.GetQuoteAsync(id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al obtener cotización: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
    }

    public async Task<QuoteDto?> CreateQuoteAsync(CreateQuoteDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            var created = await _apiClient.CreateQuoteAsync(dto);
            return created;
        }
        catch (ApiException ex)
        {
            SetError($"Error al crear cotización: {ex.Message}");
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

    public async Task<QuoteDto?> UpdateQuoteAsync(Guid id, UpdateQuoteDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            var updated = await _apiClient.UpdateQuoteAsync(id, dto);
            return updated;
        }
        catch (ApiException ex)
        {
            SetError($"Error al actualizar cotización: {ex.Message}");
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

    public async Task<QuoteDto?> SendQuoteAsync(Guid id)
    {
        ClearError();
        try
        {
            return await _apiClient.SendQuoteAsync(id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al enviar cotización: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
    }

    public async Task<QuoteDto?> AcceptQuoteAsync(Guid id)
    {
        ClearError();
        try
        {
            return await _apiClient.AcceptQuoteAsync(id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al aceptar cotización: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
    }

    public async Task<QuoteDto?> CancelQuoteAsync(Guid id)
    {
        ClearError();
        try
        {
            return await _apiClient.CancelQuoteAsync(id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al cancelar cotización: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            return null;
        }
    }


    public async Task<QuoteDto?> ConvertQuoteToSaleAsync(Guid id, ConvertQuoteToSaleDto dto)
    {
        ClearError();
        IsLoading = true;

        try
        {
            return await _apiClient.ConvertQuoteToSaleAsync(id, dto);
        }
        catch (ApiException ex)
        {
            SetError($"Error al convertir la cotización: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado al convertir: {ex.Message}");
            return null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<IReadOnlyList<RaceSlotDto>> GetAvailableSlotsAsync(Guid editionId)
    {
        var result = await _apiClient.GetSlotsAsync(editionId, 1, 100);
        return result?.Items.Where(slot => slot.Status is SlotStatusEnum.Available or SlotStatusEnum.Reserved or SlotStatusEnum.Transferable).ToList()
            ?? new List<RaceSlotDto>();
    }
}
