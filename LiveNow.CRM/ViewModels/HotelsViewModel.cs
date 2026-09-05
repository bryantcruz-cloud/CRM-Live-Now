using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public sealed class HotelsViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    public HotelsViewModel(ApiClient apiClient) => _apiClient = apiClient;
    public ObservableCollection<HotelDto> Hotels { get; } = new();
    public ObservableCollection<HotelReservationDto> Reservations { get; } = new();
    public ObservableCollection<CustomerDto> Customers { get; } = new();
    public ObservableCollection<SaleDto> Sales { get; } = new();

    public async Task LoadAsync()
    {
        ClearError(); IsLoading = true;
        try
        {
            Hotels.Clear(); Reservations.Clear(); Customers.Clear(); Sales.Clear();
            var hotels = await _apiClient.GetHotelsAsync();
            var reservations = await _apiClient.GetHotelReservationsAsync();
            var customers = await _apiClient.GetCustomersAsync(page: 1, pageSize: 100);
            var sales = await _apiClient.GetSalesAsync(page: 1, pageSize: 100);
            if (hotels is not null) foreach (HotelDto hotel in hotels) Hotels.Add(hotel);
            if (reservations is not null) foreach (HotelReservationDto reservation in reservations) Reservations.Add(reservation);
            if (customers is not null) foreach (CustomerDto customer in customers.Items) Customers.Add(customer);
            if (sales is not null) foreach (SaleDto sale in sales.Items) Sales.Add(sale);
        }
        catch (Exception ex) { SetError($"Error al cargar hoteles y reservas: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    public async Task<bool> SaveHotelAsync(HotelDto? existing, CreateHotelDto create, UpdateHotelDto update)
    {
        try
        {
            HotelDto? saved = existing is null ? await _apiClient.CreateHotelAsync(create) : await _apiClient.UpdateHotelAsync(existing.Id, update);
            if (saved is null) return false;
            await LoadAsync(); return true;
        }
        catch (Exception ex) { SetError($"Error al guardar hotel: {ex.Message}"); return false; }
    }

    public async Task<bool> SaveReservationAsync(HotelReservationDto? existing, CreateHotelReservationDto create, UpdateHotelReservationDto update)
    {
        try
        {
            HotelReservationDto? saved = existing is null ? await _apiClient.CreateHotelReservationAsync(create) : await _apiClient.UpdateHotelReservationAsync(existing.Id, update);
            if (saved is null) return false;
            await LoadAsync(); return true;
        }
        catch (Exception ex) { SetError($"Error al guardar reserva: {ex.Message}"); return false; }
    }
}
