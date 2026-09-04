using System.ComponentModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class QuotesView : Page
{
    public QuotesViewModel ViewModel { get; }

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

    public QuotesView()
    {
        this.InitializeComponent();
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ApiClient apiClient)
        {
            ViewModel = new QuotesViewModel(apiClient);
            DataContext = ViewModel;
            _ = ViewModel.LoadQuotesAsync();
            _ = ViewModel.LoadLookupDataAsync();
        }
    }

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void NewQuoteButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new QuoteFormDialog(ViewModel.Customers, ViewModel.Races, ViewModel.Editions);
        dialog.XamlRoot = this.XamlRoot;
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var dto = dialog.GetCreateDto();
            if (dto != null)
            {
                var created = await ViewModel.CreateQuoteAsync(dto);
                if (created != null)
                {
                    SuccessMessage = $"Cotización #{created.QuoteNumber} creada exitosamente";
                    SuccessInfoBar.IsOpen = true;
                    await ViewModel.LoadQuotesAsync();
                }
            }
        }
    }

    private async void ViewButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var quote = await ViewModel.GetQuoteByIdAsync(id);
            if (quote != null)
            {
                var dialog = new QuoteDetailDialog(quote);
                dialog.XamlRoot = this.XamlRoot;
                await dialog.ShowAsync();
            }
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var quote = await ViewModel.GetQuoteByIdAsync(id);
            if (quote != null && (quote.Status == Core.Enums.QuoteStatusEnum.Draft || quote.Status == Core.Enums.QuoteStatusEnum.Sent))
            {
                var dialog = new QuoteFormDialog(ViewModel.Customers, ViewModel.Races, ViewModel.Editions, quote);
                dialog.XamlRoot = this.XamlRoot;
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var dto = dialog.GetUpdateDto();
                    if (dto != null)
                    {
                        var updated = await ViewModel.UpdateQuoteAsync(id, dto);
                        if (updated != null)
                        {
                            SuccessMessage = $"Cotización #{updated.QuoteNumber} actualizada exitosamente";
                            SuccessInfoBar.IsOpen = true;
                            await ViewModel.LoadQuotesAsync();
                        }
                    }
                }
            }
        }
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var result = await ViewModel.SendQuoteAsync(id);
            if (result != null)
            {
                SuccessMessage = $"Cotización #{result.QuoteNumber} enviada exitosamente";
                SuccessInfoBar.IsOpen = true;
                await ViewModel.LoadQuotesAsync();
            }
        }
    }

    private async void QuotesList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is QuoteDto quote)
        {
            ViewModel.SelectedQuote = quote;
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadQuotesAsync();
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
