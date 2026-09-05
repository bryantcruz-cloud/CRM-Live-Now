using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public class AuditViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string? _entityFilter;
    private AuditActionEnum? _actionFilter;
    private string? _fromText;
    private string? _toText;
    public AuditViewModel(ApiClient apiClient) => _apiClient = apiClient;
    public ObservableCollection<AuditLogDto> Logs { get; } = new();
    public string? EntityFilter { get => _entityFilter; set => SetProperty(ref _entityFilter, value); }
    public AuditActionEnum? ActionFilter { get => _actionFilter; set => SetProperty(ref _actionFilter, value); }
    public string? FromText { get => _fromText; set => SetProperty(ref _fromText, value); }
    public string? ToText { get => _toText; set => SetProperty(ref _toText, value); }
    public IReadOnlyList<AuditActionEnum> Actions { get; } = Enum.GetValues<AuditActionEnum>();
    public async Task LoadAsync()
    {
        IsLoading = true; ClearError();
        try { Logs.Clear(); DateTime? from = DateTime.TryParse(FromText, out DateTime fromValue) ? fromValue : null; DateTime? to = DateTime.TryParse(ToText, out DateTime toValue) ? toValue : null; IReadOnlyList<AuditLogDto>? result = await _apiClient.GetAuditLogsAsync(200, from, to, EntityFilter, ActionFilter); if (result is not null) foreach (AuditLogDto log in result) Logs.Add(log); }
        catch (Exception ex) { SetError($"No se pudo cargar la auditoría: {ex.Message}"); }
        finally { IsLoading = false; }
    }
}
