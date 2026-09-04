using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the Races module.
/// Handles listing races, searching, and viewing editions of a selected race.
/// </summary>
public class RacesViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    private ObservableCollection<RaceDto> _races = new();
    private ObservableCollection<RaceEditionDto> _editions = new();
    private string _searchText = string.Empty;
    private RaceDto? _selectedRace;
    private bool _isEditionsLoading;

    public RacesViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    #region Properties

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

    public bool IsEditionsLoading
    {
        get => _isEditionsLoading;
        private set => SetProperty(ref _isEditionsLoading, value);
    }

    public bool HasRaces => Races.Count > 0;
    public bool HasEditions => Editions.Count > 0;

    #endregion

    #region Methods

    public async Task LoadRacesAsync()
    {
        ClearError();
        IsLoading = true;

        try
        {
            var races = await _apiClient.GetRacesAsync();

            if (races is not null)
            {
                var filtered = FilterRaces(races);
                Races = new ObservableCollection<RaceDto>(filtered);
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

    private IEnumerable<RaceDto> FilterRaces(IEnumerable<RaceDto> races)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return races;
        }

        string search = SearchText.Trim().ToLowerInvariant();
        return races.Where(r =>
            r.Name.ToLowerInvariant().Contains(search) ||
            r.City.ToLowerInvariant().Contains(search) ||
            r.Country.ToLowerInvariant().Contains(search) ||
            r.RaceType.ToString().ToLowerInvariant().Contains(search));
    }

    public async Task SearchAsync()
    {
        await LoadRacesAsync();
    }

    private async Task LoadEditionsAsync()
    {
        if (SelectedRace is null)
        {
            Editions = new ObservableCollection<RaceEditionDto>();
            OnPropertyChanged(nameof(HasEditions));
            return;
        }

        IsEditionsLoading = true;
        OnPropertyChanged(nameof(HasEditions));

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
        }
        catch (ApiException ex)
        {
            SetError($"Error al cargar ediciones: {ex.Message}");
            Editions = new ObservableCollection<RaceEditionDto>();
        }
        catch (Exception ex)
        {
            SetError($"Error inesperado: {ex.Message}");
            Editions = new ObservableCollection<RaceEditionDto>();
        }
        finally
        {
            IsEditionsLoading = false;
            OnPropertyChanged(nameof(HasEditions));
        }
    }

    #endregion
}
