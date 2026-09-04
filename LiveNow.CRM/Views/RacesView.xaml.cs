using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class RacesView : Page
{
    public RacesViewModel ViewModel { get; }

    public RacesView()
    {
        this.InitializeComponent();
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ApiClient apiClient)
        {
            ViewModel = new RacesViewModel(apiClient);
            DataContext = ViewModel;
            _ = ViewModel.LoadRacesAsync();
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadRacesAsync();
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SearchAsync();
    }

    private async void SearchBox_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await ViewModel.SearchAsync();
    }

    private void InfoBar_CloseButtonClick(InfoBar sender, object args)
    {
        ViewModel.ClearError();
    }
}
