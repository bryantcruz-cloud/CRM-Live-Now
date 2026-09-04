using System.ComponentModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Services;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class CustomersView : Page
{
    public CustomersViewModel ViewModel { get; }

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

    public CustomersView()
    {
        this.InitializeComponent();
        ViewModel = null!;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ApiClient apiClient)
        {
            ViewModel = new CustomersViewModel(apiClient);
            DataContext = ViewModel;
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            _ = ViewModel.LoadCustomersAsync();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CustomersViewModel.HasPreviousPage) or nameof(CustomersViewModel.HasNextPage) or nameof(CustomersViewModel.PageInfo))
        {
            OnPropertyChanged(nameof(SuccessMessage));
        }
    }

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void NewCustomerButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CustomerFormDialog();
        dialog.XamlRoot = this.XamlRoot;
        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var dto = dialog.GetCreateDto();
            if (dto != null)
            {
                var created = await ViewModel.CreateCustomerAsync(dto);
                if (created != null)
                {
                    SuccessMessage = "Cliente creado exitosamente";
                    SuccessInfoBar.IsOpen = true;
                    await ViewModel.LoadCustomersAsync();
                }
            }
        }
    }

    private async void EditButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var customer = await ViewModel.GetCustomerByIdAsync(id);
            if (customer != null)
            {
                var dialog = new CustomerFormDialog(customer);
                dialog.XamlRoot = this.XamlRoot;
                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var dto = dialog.GetUpdateDto();
                    if (dto != null)
                    {
                        var updated = await ViewModel.UpdateCustomerAsync(id, dto);
                        if (updated != null)
                        {
                            SuccessMessage = "Cliente actualizado exitosamente";
                            SuccessInfoBar.IsOpen = true;
                            await ViewModel.LoadCustomersAsync();
                        }
                    }
                }
            }
        }
    }

    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string idStr && Guid.TryParse(idStr, out Guid id))
        {
            var confirmDialog = new ContentDialog
            {
                Title = "Confirmar eliminación",
                Content = "¿Está seguro de eliminar este cliente? Esta acción no se puede deshacer.",
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                XamlRoot = this.XamlRoot
            };

            var result = await confirmDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var deleted = await ViewModel.DeleteCustomerAsync(id);
                if (deleted)
                {
                    SuccessMessage = "Cliente eliminado exitosamente";
                    SuccessInfoBar.IsOpen = true;
                    await ViewModel.LoadCustomersAsync();
                }
            }
        }
    }

    private async void CustomersList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is CustomerDto customer)
        {
            ViewModel.SelectedCustomer = customer;
        }
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.SearchAsync();
    }

    private async void SearchBox_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        await ViewModel.SearchAsync();
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCustomersAsync();
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
