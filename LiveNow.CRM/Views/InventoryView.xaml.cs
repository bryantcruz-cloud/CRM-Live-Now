using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class InventoryView : Page
{
    public InventoryViewModel ViewModel { get; }

    public InventoryView()
    {
        this.InitializeComponent();
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ApiClient apiClient)
        {
            ViewModel = new InventoryViewModel(apiClient);
            DataContext = ViewModel;
            _ = ViewModel.LoadRacesAsync();
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.RefreshAllAsync();
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SearchAsync();
    }

    private async void SearchBox_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await ViewModel.SearchAsync();
    }

    private async void FirstPageButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.GoToFirstPageAsync();
    }

    private async void PreviousPageButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.GoToPreviousPageAsync();
    }

    private async void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.GoToNextPageAsync();
    }

    private async void LastPageButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.GoToLastPageAsync();
    }

    private void InfoBar_CloseButtonClick(InfoBar sender, object args)
    {
        ViewModel.ClearError();
    }
}
