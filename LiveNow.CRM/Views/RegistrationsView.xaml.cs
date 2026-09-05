using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LiveNow.CRM.Views;

public sealed partial class RegistrationsView : Page
{
    public RegistrationsViewModel ViewModel { get; private set; } = null!;
    public RegistrationDto? SelectedRegistration { get; set; }
    public RegistrationsView() => InitializeComponent();
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is LiveNow.CRM.Services.ApiClient apiClient) { ViewModel = new RegistrationsViewModel(apiClient); await ViewModel.LoadAsync(); }
    }

    private async void New_Click(object sender, RoutedEventArgs e)
    {
        ComboBox customer = new() { Header = "Cliente", ItemsSource = ViewModel.Customers, DisplayMemberPath = "DisplayName", SelectedIndex = 0 }; ComboBox edition = new() { Header = "Edición", ItemsSource = ViewModel.Editions, DisplayMemberPath = "Year", SelectedIndex = 0 }; ComboBox slot = new() { Header = "Plaza", ItemsSource = ViewModel.Slots, DisplayMemberPath = "InternalCode", SelectedIndex = 0 }; ComboBox status = new() { ItemsSource = ViewModel.StatusOptions, SelectedIndex = 0 }; TextBox confirmation = new() { PlaceholderText = "Confirmación" }; TextBox notes = new() { PlaceholderText = "Notas" };
        ContentDialog dialog = new() { Title = "Nuevo registro", Content = new StackPanel { Spacing = 8, Children = { customer, edition, slot, status, confirmation, notes } }, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary || customer.SelectedItem is not CustomerDto customerDto || edition.SelectedItem is not RaceEditionDto editionDto || slot.SelectedItem is not RaceSlotDto slotDto) return;
        await ViewModel.SaveAsync(null, new CreateRegistrationDto { CustomerId = customerDto.Id, RaceEditionId = editionDto.Id, RaceSlotId = slotDto.Id, RegistrationStatus = (RegistrationStatusEnum)status.SelectedItem, ConfirmationNumber = confirmation.Text, Notes = notes.Text }, new UpdateRegistrationDto());
    }

    private async void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedRegistration is null) return;
        ComboBox status = new() { ItemsSource = ViewModel.StatusOptions, SelectedItem = SelectedRegistration.RegistrationStatus }; TextBox confirmation = new() { Text = SelectedRegistration.ConfirmationNumber }; TextBox notes = new() { Text = SelectedRegistration.Notes };
        ContentDialog dialog = new() { Title = "Editar registro", Content = new StackPanel { Spacing = 8, Children = { status, confirmation, notes } }, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.SaveAsync(SelectedRegistration, new CreateRegistrationDto(), new UpdateRegistrationDto { RegistrationStatus = (RegistrationStatusEnum)status.SelectedItem, RegistrationDate = SelectedRegistration.RegistrationDate, ConfirmationNumber = confirmation.Text, Notes = notes.Text });
    }
}
