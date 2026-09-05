using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LiveNow.CRM.Views;

public sealed partial class AuditView : Page
{
    public AuditViewModel ViewModel { get; private set; } = null!;
    public AuditView() => InitializeComponent();
    protected override async void OnNavigatedTo(NavigationEventArgs e) { base.OnNavigatedTo(e); if (e.Parameter is ApiClient apiClient) { ViewModel = new AuditViewModel(apiClient); await ViewModel.LoadAsync(); } }
    private async void Filter_Click(object sender, RoutedEventArgs e) => await ViewModel.LoadAsync();
}
