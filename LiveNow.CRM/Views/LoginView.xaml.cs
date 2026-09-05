using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class LoginView : Page
{
    private readonly LoginViewModel _viewModel;
    private readonly ApiClient _apiClient;

    public event EventHandler? LoggedIn;

    public LoginView(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _viewModel = new LoginViewModel(apiClient);
        InitializeComponent();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        LoginButton.IsEnabled = false;
        ErrorText.Text = string.Empty;
        bool success = await _viewModel.LoginAsync(UserOrEmailBox.Text.Trim(), PasswordBox.Password);
        PasswordBox.Password = string.Empty;
        ErrorText.Text = _viewModel.ErrorMessage ?? string.Empty;
        LoginButton.IsEnabled = true;
        if (success) LoggedIn?.Invoke(this, EventArgs.Empty);
    }
}
