using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the Inventory module.
/// Shows race slots with filtering by race, edition, and status.
/// Displays inventory summary with counts per status.
/// </summary>
public class InventoryViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    private ObservableCollection<RaceDto> _races = new();
    private ObservableCollection<RaceEditionDto> _editions = new();
    private ObservableCollection<RaceSlotDto> _slots = new();
    private InventorySummaryDto? _inventorySummary;

    private string _searchText = string.Empty;
    private RaceDto? _selectedRace;
    private RaceEditionDto? _selectedEdition;
    private SlotStatusEnum? _selectedStatusFilter;
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int _totalCount;
    private int _totalPages;
    private bool _isSlotsLoading;
    private bool _isSummaryLoading;

    public InventoryViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
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

    public ObservableCollection<RaceSlotDto> Slots
    {
        get => _slots;
        private set => SetProperty(ref _slots, value);
    }

    public InventorySummaryDto? InventorySummary
    {
        get => _inventorySummary;
        private set => SetProperty(ref _inventorySummary, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public RaceDto? SelectedRace
    {
        get => _selectedRace;
        set
        {
            if (SetProperty(ref _selectedRace, value))
            {
                _ = LoadEditionsAsync();
            }
        }
    }

    public RaceEditionDto? SelectedEdition
    {
        get => _selectedEdition;
        set
        {
            if (SetProperty(ref _selectedEdition, value))
            {
                _ = LoadSlotsAsync();
                _ = LoadInventorySummaryAsync();
            }
        }
    }

    public SlotStatusEnum? SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set
        {
            if (SetProperty(ref _selectedStatusFilter, value))
            {
                _ = LoadSlotsAsync();
            }
        }
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

    public bool IsSlotsLoading
    {
        get => _isSlotsLoading;
        private set => SetProperty(ref _isSlotsLoading, value);
    }

    public bool IsSummaryLoading
    {
        get => _isSummaryLoading;
        private set => SetProperty(ref _isSummaryLoading, value);
    }

    public bool HasSlots => Slots.Count > 0;
    public bool HasRaces => Races.Count > 0;
    public bool HasEditions => Editions.Count > 0;
    public bool HasEditionSelected => SelectedEdition != null;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public string PageInfo => TotalCount == 0
        ? "Sin resultados"
        : $"Página {CurrentPage} de {TotalPages} ({TotalCount} plazas)";

    public IReadOnlyList<SlotStatusEnum> StatusFilterOptions { get; } = new List<SlotStatusEnum>
    {
        SlotStatusEnum.Available,
        SlotStatusEnum.Reserved,
        SlotStatusEnum.Sold,
        SlotStatusEnum.Registered,
        SlotStatusEnum.Cancelled,
        SlotStatusEnum.Injured,
        SlotStatusEnum.Transferable,
        SlotStatusEnum.Lost
    };

    public async Task LoadRacesAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            var races = await _apiClient.GetRacesAsync();

            if (races is not null)
            {
                Races = new ObservableCollection<RaceDto>(races);
            }
            else
            {
                Races = new ObservableCollection<RaceDto>();
            }

            OnPropertyChanged(nameof(HasRaces));
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar carreras: {ex.Message}");
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

    private async Task LoadEditionsAsync()
    {
        if (SelectedRace is null)
        {
            Editions = new ObservableCollection<RaceEditionDto>();
            OnPropertyChanged(nameof(HasEditions));
            return;
        }

        try
        {
            var editions = await _apiClient.GetEditionsAsync(SelectedRace.Id);

            if (editions is not null)
            {
                Editions = new ObservableCollection<RaceEditionDto>(editions);
            }
            else
            {
                Editions = new ObservableCollection<RaceEditionDto>();
            }

            OnPropertyChanged(nameof(HasEditions));
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar ediciones: {ex.Message}");
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
        }
    }

    public async Task LoadSlotsAsync()
    {
        if (SelectedEdition is null)
        {
            Slots = new ObservableCollection<RaceSlotDto>();
            TotalCount = 0;
            TotalPages = 0;
            OnPropertyChanged(nameof(HasSlots));
            OnPropertyChanged(nameof(HasPreviousPage));
            OnPropertyChanged(nameof(HasNextPage));
            OnPropertyChanged(nameof(PageInfo));
            return;
        }

        IsSlotsLoading = true;
        CurrentPage = 1;

        await FetchSlotsAsync();

        IsSlotsLoading = false;
        OnPropertyChanged(nameof(HasSlots));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(PageInfo));
    }

    private async Task FetchSlotsAsync()
    {
        try
        {
            var result = await _apiClient.GetSlotsAsync(
                SelectedEdition!.Id,
                CurrentPage,
                PageSize,
                SelectedStatusFilter);

            if (result is not null)
            {
                var filtered = FilterSlots(result.Items);
                Slots = new ObservableCollection<RaceSlotDto>(filtered);
                TotalCount = result.TotalCount;
                TotalPages = result.TotalPages;
            }
            else
            {
                Slots = new ObservableCollection<RaceSlotDto>();
                TotalCount = 0;
                TotalPages = 0;
            }
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar plazas: {ex.Message}");
            Slots = new ObservableCollection<RaceSlotDto>();
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            Slots = new ObservableCollection<RaceSlotDto>();
        }
    }

    private IEnumerable<RaceSlotDto> FilterSlots(IEnumerable<RaceSlotDto> slots)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return slots;
        }

        string search = SearchText.Trim().ToLowerInvariant();
        return slots.Where(s => s.InternalCode.ToLowerInvariant().Contains(search));
    }

    private async Task LoadInventorySummaryAsync()
    {
        if (SelectedEdition is null)
        {
            InventorySummary = null;
            return;
        }

        IsSummaryLoading = true;

        try
        {
            InventorySummary = await _apiClient.GetInventorySummaryAsync(SelectedEdition.Id);
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar resumen: {ex.Message}");
            InventorySummary = null;
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            InventorySummary = null;
        }
        finally
        {
            IsSummaryLoading = false;
        }
    }

    public async Task SearchAsync()
    {
        await LoadSlotsAsync();
    }

    public async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        CurrentPage = page;
        await FetchSlotsAsync();
        OnPropertyChanged(nameof(HasSlots));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
        OnPropertyChanged(nameof(PageInfo));
    }

    public async Task GoToFirstPageAsync() => await GoToPageAsync(1);
    public async Task GoToPreviousPageAsync() => await GoToPageAsync(CurrentPage - 1);
    public async Task GoToNextPageAsync() => await GoToPageAsync(CurrentPage + 1);
    public async Task GoToLastPageAsync() => await GoToPageAsync(TotalPages);

    public async Task RefreshAllAsync()
    {
        await LoadRacesAsync();
        if (SelectedEdition != null)
        {
            await LoadSlotsAsync();
            await LoadInventorySummaryAsync();
        }
    }
}
