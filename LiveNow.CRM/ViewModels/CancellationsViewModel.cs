using System.Collections.ObjectModel;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public sealed class CancellationsViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private CustomerDto? _selectedCancellationCustomer;
    private SaleOption? _selectedCancellationSale;
    private SlotOption? _selectedCancellationSlot;
    private SlotOption? _selectedTransferSlot;
    private CustomerDto? _selectedTransferCustomer;

    public CancellationsViewModel(ApiClient apiClient) => _apiClient = apiClient;
    public ObservableCollection<CancellationDto> Cancellations { get; } = new();
    public ObservableCollection<SlotTransferDto> Transfers { get; } = new();
    public ObservableCollection<CustomerDto> Customers { get; } = new();
    public ObservableCollection<SaleOption> Sales { get; } = new();
    public ObservableCollection<SlotOption> Slots { get; } = new();
    public IEnumerable<SaleOption> FilteredSales => SelectedCancellationCustomer is null ? Sales : Sales.Where(s => s.CustomerId == SelectedCancellationCustomer.Id);
    public IEnumerable<SlotOption> FilteredSlots => SelectedCancellationSale is null ? Slots : Slots.Where(s => SelectedCancellationSale.SlotIds.Contains(s.Id));
    public IEnumerable<SlotOption> TransferableSlots => Slots.Where(s => s.Status is SlotStatusEnum.Transferable or SlotStatusEnum.Reserved);
    public CustomerDto? SelectedCancellationCustomer { get => _selectedCancellationCustomer; set { if (SetProperty(ref _selectedCancellationCustomer, value)) { SelectedCancellationSale = null; SelectedCancellationSlot = null; OnPropertyChanged(nameof(FilteredSales)); OnPropertyChanged(nameof(FilteredSlots)); } } }
    public SaleOption? SelectedCancellationSale { get => _selectedCancellationSale; set { if (SetProperty(ref _selectedCancellationSale, value)) { SelectedCancellationSlot = null; OnPropertyChanged(nameof(FilteredSlots)); } } }
    public SlotOption? SelectedCancellationSlot { get => _selectedCancellationSlot; set => SetProperty(ref _selectedCancellationSlot, value); }
    public SlotOption? SelectedTransferSlot { get => _selectedTransferSlot; set => SetProperty(ref _selectedTransferSlot, value); }
    public CustomerDto? SelectedTransferCustomer { get => _selectedTransferCustomer; set => SetProperty(ref _selectedTransferCustomer, value); }

    public async Task LoadAsync()
    {
        IsLoading = true; ClearError();
        try
        {
            IReadOnlyList<CancellationDto>? cancellations = await _apiClient.GetCancellationsAsync();
            IReadOnlyList<SlotTransferDto>? transfers = await _apiClient.GetTransfersAsync();
            PagedResult<CustomerDto>? customers = await _apiClient.GetCustomersAsync(page: 1, pageSize: 100);
            PagedResult<SaleDto>? sales = await _apiClient.GetSalesAsync(page: 1, pageSize: 100);
            IReadOnlyList<RaceDto>? races = await _apiClient.GetRacesAsync();
            List<SlotOption> slots = new();
            if (races is not null)
            {
                foreach (RaceDto race in races)
                {
                    IReadOnlyList<RaceEditionDto>? raceEditions = await _apiClient.GetEditionsAsync(race.Id);
                    if (raceEditions is null) continue;
                    foreach (RaceEditionDto edition in raceEditions)
                    {
                        PagedResult<RaceSlotDto>? page = await _apiClient.GetSlotsAsync(edition.Id, 1, 100);
                        if (page is not null) slots.AddRange(page.Items.Select(slot => new SlotOption(slot, $"{slot.InternalCode} · {race.Name} {edition.Year}")));
                    }
                }
            }
            Cancellations.Clear(); Transfers.Clear(); Customers.Clear(); Sales.Clear(); Slots.Clear();
            if (cancellations is not null) foreach (CancellationDto item in cancellations) Cancellations.Add(item);
            if (transfers is not null) foreach (SlotTransferDto item in transfers) Transfers.Add(item);
            if (customers is not null) foreach (CustomerDto item in customers.Items) Customers.Add(item);
            if (sales is not null) foreach (SaleDto sale in sales.Items) Sales.Add(new SaleOption(sale));
            foreach (SlotOption slot in slots) Slots.Add(slot);
            OnPropertyChanged(nameof(FilteredSales)); OnPropertyChanged(nameof(FilteredSlots)); OnPropertyChanged(nameof(TransferableSlots));
        }
        catch (Exception ex) { SetError($"No se pudo cargar cancelaciones y transferencias: {ex.Message}"); }
        finally { IsLoading = false; }
    }
    public async Task<bool> CancelAsync(bool injury, bool reassignable, string reason)
    {
        if (SelectedCancellationCustomer is null) return false;
        try { await _apiClient.CreateCancellationAsync(new CreateCancellationDto { CustomerId = SelectedCancellationCustomer.Id, SaleId = SelectedCancellationSale?.Id, RaceSlotId = SelectedCancellationSlot?.Id, Reason = reason, IsInjury = injury, CanReassignSlot = reassignable }); await LoadAsync(); return true; }
        catch (Exception ex) { SetError($"No se pudo registrar la cancelación: {ex.Message}"); return false; }
    }
    public async Task<bool> TransferAsync(string reason)
    {
        if (SelectedTransferSlot is null || SelectedTransferCustomer is null) return false;
        try { await _apiClient.TransferSlotAsync(SelectedTransferSlot.Id, new SlotTransferRequestDto { ToCustomerId = SelectedTransferCustomer.Id, Reason = reason }); await LoadAsync(); return true; }
        catch (Exception ex) { SetError($"No se pudo transferir la plaza: {ex.Message}"); return false; }
    }
}

public sealed class SaleOption
{
    public SaleOption(SaleDto sale) { Id = sale.Id; CustomerId = sale.CustomerId; SaleNumber = sale.SaleNumber; CustomerName = sale.CustomerName; Status = sale.Status; SlotIds = sale.Items.Where(i => i.RaceSlotId.HasValue).Select(i => i.RaceSlotId!.Value).ToHashSet(); }
    public Guid Id { get; }
    public Guid CustomerId { get; }
    public string SaleNumber { get; }
    public string CustomerName { get; }
    public SaleStatusEnum Status { get; }
    public HashSet<Guid> SlotIds { get; }
    public string DisplayName => $"{SaleNumber} · {CustomerName} · {Status}";
}

public sealed class SlotOption
{
    public SlotOption(RaceSlotDto slot, string displayName) { Id = slot.Id; Status = slot.Status; AssignedCustomerId = slot.AssignedCustomerId; DisplayName = $"{displayName} · {slot.Status}"; }
    public Guid Id { get; }
    public SlotStatusEnum Status { get; }
    public Guid? AssignedCustomerId { get; }
    public string DisplayName { get; }
}
