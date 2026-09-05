using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

/// <summary>
/// ViewModel for the main shell (navigation window).
/// Manages navigation state and the ApiClient instance.
/// </summary>
public class ShellViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;
    private string _apiStatus = "Desconectado";
    private bool _isApiOnline;

    public ShellViewModel()
    {
        string? configuredApiUrl = Environment.GetEnvironmentVariable("LIVENOW_API_URL");
        _apiClient = new ApiClient(string.IsNullOrWhiteSpace(configuredApiUrl) ? "https://localhost:5001" : configuredApiUrl);
    }

    public ApiClient ApiClient => _apiClient;

    public string ApiStatus
    {
        get => _apiStatus;
        set => SetProperty(ref _apiStatus, value);
    }

    public bool IsApiOnline
    {
        get => _isApiOnline;
        set => SetProperty(ref _isApiOnline, value);
    }

    public async Task CheckApiHealthAsync()
    {
        try
        {
            IsApiOnline = await _apiClient.IsHealthyAsync();
            ApiStatus = IsApiOnline ? "Conectado" : "Sin respuesta";
        }
        catch
        {
            IsApiOnline = false;
            ApiStatus = "Desconectado";
        }
    }
}
