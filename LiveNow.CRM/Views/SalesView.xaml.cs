using System.ComponentModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class SalesView : Page
{
    public SalesViewModel ViewModel { get; }

    private string _successMessage = string.Empty;
    public string SuccessMessage
    {
        get => _successMessage;
        set
        {
            _successMessage = value;
            OnPropertyChanged();
        }
    }

    public SalesView()
    {
        this.InitializeComponent();
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ApiClient apiClient)
        {
            ViewModel = new SalesViewModel(apiClient);
            DataContext = ViewModel;
            _ = ViewModel.LoadSalesAsync();
            _ = ViewModel.LoadLookupDataAsync();
        }
    }

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void NewSaleButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaleFormDialog(ViewModel.Customers, ViewModel.Races, ViewModel.Editions);
        dialog.XamlRoot = this.XamlRoot;
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var dto = dialog.GetCreateDto();
            if (dto != null)
            {
                var created = await ViewModel.CreateSaleAsync(dto);
                if (created != null)
                {
                    SuccessMessage = $"Venta #{created.SaleNumber} creada exitosamente";
                    SuccessInfoBar.IsOpen = true;
                    await ViewModel.LoadSalesAsync();
                }
            }
        }
    }

    private async void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var sale = await ViewModel.GetSaleByIdAsync(id);
            if (sale != null)
            {
                await ViewModel.LoadFinancialSummaryAsync(id);
                var dialog = new SaleDetailDialog(sale, ViewModel.FinancialSummary);
                dialog.XamlRoot = this.XamlRoot;
                await dialog.ShowAsync();
            }
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var sale = await ViewModel.GetSaleByIdAsync(id);
            if (sale != null && sale.Status == Core.Enums.SaleStatusEnum.Pending)
            {
                var dialog = new SaleFormDialog(ViewModel.Customers, ViewModel.Races, ViewModel.Editions, sale);
                dialog.XamlRoot = this.XamlRoot;
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var dto = dialog.GetUpdateDto();
                    if (dto != null)
                    {
                        var updated = await ViewModel.UpdateSaleAsync(id, dto);
                        if (updated != null)
                        {
                            SuccessMessage = $"Venta #{updated.SaleNumber} actualizada exitosamente";
                            SuccessInfoBar.IsOpen = true;
                            await ViewModel.LoadSalesAsync();
                        }
                    }
                }
            }
        }
    }

    private async void SalesList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is SaleDto sale)
        {
            ViewModel.SelectedSale = sale;
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadSalesAsync();
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
