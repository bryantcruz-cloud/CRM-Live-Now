using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LiveNow.CRM.Views;

public sealed partial class CancellationsView : Page
{
    public CancellationsViewModel ViewModel { get; private set; } = null!;
    public CancellationsView() => InitializeComponent();
    protected override async void OnNavigatedTo(NavigationEventArgs e) { base.OnNavigatedTo(e); if (e.Parameter is ApiClient apiClient) { ViewModel = new CancellationsViewModel(apiClient); await ViewModel.LoadAsync(); } }
    private async void Cancel_Click(object sender, RoutedEventArgs e) => await ViewModel.CancelAsync(CancellationInjury.IsChecked == true, CancellationReassign.IsChecked == true, CancellationReason.Text);
    private async void Transfer_Click(object sender, RoutedEventArgs e) => await ViewModel.TransferAsync(TransferReason.Text);
    private void CancellationCustomer_Changed(object sender, SelectionChangedEventArgs e) { }
    private void CancellationSale_Changed(object sender, SelectionChangedEventArgs e) { }
}
