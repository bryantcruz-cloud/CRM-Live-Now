using Microsoft.UI.Xaml;
using LiveNow.CRM.Services;
using LiveNow.CRM.Views;

namespace LiveNow.CRM;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        string? configuredApiUrl = Environment.GetEnvironmentVariable("LIVENOW_API_URL");
        ApiClient apiClient = new(string.IsNullOrWhiteSpace(configuredApiUrl) ? "https://localhost:5001" : configuredApiUrl);
        LoginView loginView = new(apiClient);
        loginView.LoggedIn += (_, _) => Content = new ShellView(apiClient);
        Content = loginView;
    }
}
