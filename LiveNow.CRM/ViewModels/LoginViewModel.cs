using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;

namespace LiveNow.CRM.ViewModels;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly ApiClient _apiClient;

    public LoginViewModel(ApiClient apiClient) => _apiClient = apiClient;

    public async Task<bool> LoginAsync(string userOrEmail, string password)
    {
        ClearError();
        IsLoading = true;
        try
        {
            LoginResponseDto? response = await _apiClient.LoginAsync(new LoginRequestDto { UserOrEmail = userOrEmail, Password = password });
            return response is not null;
        }
        catch (ApiException)
        {
            SetError("No se pudo iniciar sesión. Verifique sus credenciales.");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
