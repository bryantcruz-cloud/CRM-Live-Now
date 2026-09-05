using System.Collections.ObjectModel;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public class TrackingViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private CustomerDto? _selectedCustomer;
    public TrackingViewModel(ApiClient apiClient) => _apiClient = apiClient;
    public ObservableCollection<CustomerDto> Customers { get; } = new();
    public ObservableCollection<ChecklistItemDto> Items { get; } = new();
    public CustomerDto? SelectedCustomer { get => _selectedCustomer; set => SetProperty(ref _selectedCustomer, value); }
    public int PendingCount => Items.Count(i => !i.IsCompleted);
    public async Task LoadAsync()
    {
        IsLoading = true; ClearError();
        try { Customers.Clear(); PagedResult<CustomerDto>? result = await _apiClient.GetCustomersAsync(page: 1, pageSize: 100); if (result is not null) foreach (CustomerDto customer in result.Items) Customers.Add(customer); SelectedCustomer ??= Customers.FirstOrDefault(); await LoadChecklistAsync(); }
        catch (Exception ex) { SetError($"No se pudo cargar el seguimiento: {ex.Message}"); }
        finally { IsLoading = false; }
    }
    public async Task LoadChecklistAsync()
    {
        if (SelectedCustomer is null) return;
        try { IReadOnlyList<ChecklistItemDto>? result = await _apiClient.GetChecklistAsync(SelectedCustomer.Id); Items.Clear(); if (result is not null) foreach (ChecklistItemDto item in result) Items.Add(item); OnPropertyChanged(nameof(PendingCount)); }
        catch (Exception ex) { SetError($"No se pudo cargar el checklist: {ex.Message}"); }
    }
    public async Task ToggleAsync(ChecklistItemDto item)
    {
        ChecklistItemDto? updated = await _apiClient.UpdateChecklistAsync(item.Id, new UpdateChecklistItemDto { ItemType = item.ItemType, Description = item.Description, IsCompleted = item.IsCompleted, Notes = item.Notes });
        if (updated is not null) { int index = Items.IndexOf(item); if (index >= 0) Items[index] = updated; OnPropertyChanged(nameof(PendingCount)); }
    }
}
