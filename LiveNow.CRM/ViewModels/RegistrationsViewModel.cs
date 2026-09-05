using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public sealed class RegistrationsViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string _searchText = string.Empty;
    private RegistrationStatusEnum? _statusFilter;
    public RegistrationsViewModel(ApiClient apiClient) => _apiClient = apiClient;
    public ObservableCollection<RegistrationDto> Registrations { get; } = new();
    public ObservableCollection<CustomerDto> Customers { get; } = new();
    public ObservableCollection<RaceEditionDto> Editions { get; } = new();
    public ObservableCollection<RaceSlotDto> Slots { get; } = new();
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) OnPropertyChanged(nameof(FilteredRegistrations)); } }
    public RegistrationStatusEnum? StatusFilter { get => _statusFilter; set { if (SetProperty(ref _statusFilter, value)) OnPropertyChanged(nameof(FilteredRegistrations)); } }
    public IReadOnlyList<RegistrationStatusEnum> StatusOptions { get; } = Enum.GetValues<RegistrationStatusEnum>();
    public IEnumerable<RegistrationDto> FilteredRegistrations => Registrations.Where(MatchesFilter);

    private bool MatchesFilter(RegistrationDto item)
    {
        bool status = !StatusFilter.HasValue || item.RegistrationStatus == StatusFilter.Value;
        string search = SearchText.Trim();
        return status && (search.Length == 0 || item.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase) || item.RaceEditionName.Contains(search, StringComparison.OrdinalIgnoreCase) || item.RaceSlotCode.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    public async Task LoadAsync()
    {
        ClearError(); IsLoading = true;
        try
        {
            Registrations.Clear(); Customers.Clear(); Editions.Clear(); Slots.Clear();
            var result = await _apiClient.GetRegistrationsAsync();
            var customers = await _apiClient.GetCustomersAsync(page: 1, pageSize: 100);
            var races = await _apiClient.GetRacesAsync();
            if (result is not null) foreach (RegistrationDto item in result) Registrations.Add(item);
            if (customers is not null) foreach (CustomerDto customer in customers.Items) Customers.Add(customer);
            if (races is not null)
            {
                var editionResults = await Task.WhenAll(races.Select(race => _apiClient.GetEditionsAsync(race.Id)));
                foreach (var editions in editionResults.Where(items => items is not null)) foreach (RaceEditionDto edition in editions!) Editions.Add(edition);
                var slotResults = await Task.WhenAll(Editions.Select(edition => _apiClient.GetSlotsAsync(edition.Id, 1, 100)));
                foreach (var slots in slotResults.Where(items => items is not null)) foreach (RaceSlotDto slot in slots!.Items) Slots.Add(slot);
            }
            OnPropertyChanged(nameof(FilteredRegistrations));
        }
        catch (Exception ex) { SetError($"Error al cargar registros: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    public async Task<bool> SaveAsync(RegistrationDto? existing, CreateRegistrationDto create, UpdateRegistrationDto update)
    {
        try
        {
            RegistrationDto? saved = existing is null ? await _apiClient.CreateRegistrationAsync(create) : await _apiClient.UpdateRegistrationAsync(existing.Id, update);
            if (saved is null) return false;
            await LoadAsync(); return true;
        }
        catch (Exception ex) { SetError($"Error al guardar registro: {ex.Message}"); return false; }
    }
}
