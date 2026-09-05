using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LiveNow.CRM.Views;

public sealed partial class TrackingView : Page
{
    public TrackingViewModel ViewModel { get; private set; } = null!;
    public TrackingView() => InitializeComponent();
    protected override async void OnNavigatedTo(NavigationEventArgs e) { base.OnNavigatedTo(e); if (e.Parameter is ApiClient apiClient) { ViewModel = new TrackingViewModel(apiClient); await ViewModel.LoadAsync(); } }
    private async void Customer_Changed(object sender, SelectionChangedEventArgs e) => await ViewModel.LoadChecklistAsync();
    private async void Checklist_Click(object sender, RoutedEventArgs e) { if (sender is CheckBox checkBox && checkBox.DataContext is ChecklistItemDto item) await ViewModel.ToggleAsync(item); }
}
