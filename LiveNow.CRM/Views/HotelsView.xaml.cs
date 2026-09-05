using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace LiveNow.CRM.Views;

public sealed partial class HotelsView : Page
{
    public HotelsViewModel ViewModel { get; private set; } = null!;
    public HotelDto? SelectedHotel { get; set; }
    public HotelReservationDto? SelectedReservation { get; set; }
    public HotelsView() => InitializeComponent();
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is LiveNow.CRM.Services.ApiClient apiClient) { ViewModel = new HotelsViewModel(apiClient); await ViewModel.LoadAsync(); }
    }

    private async void NewHotel_Click(object sender, RoutedEventArgs e)
    {
        TextBox name = new() { PlaceholderText = "Nombre" }; TextBox city = new() { PlaceholderText = "Ciudad" }; TextBox country = new() { PlaceholderText = "País" }; TextBox notes = new() { PlaceholderText = "Notas" };
        ContentDialog dialog = new() { Title = "Nuevo hotel", Content = new StackPanel { Spacing = 8, Children = { name, city, country, notes } }, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.SaveHotelAsync(null, new CreateHotelDto { Name = name.Text, City = city.Text, Country = country.Text, Notes = notes.Text }, new UpdateHotelDto());
    }

    private async void EditHotel_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedHotel is null) return;
        TextBox name = new() { Text = SelectedHotel.Name }; TextBox city = new() { Text = SelectedHotel.City }; TextBox country = new() { Text = SelectedHotel.Country }; TextBox notes = new() { Text = SelectedHotel.Notes };
        ContentDialog dialog = new() { Title = "Editar hotel", Content = new StackPanel { Spacing = 8, Children = { name, city, country, notes } }, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.SaveHotelAsync(SelectedHotel, new CreateHotelDto(), new UpdateHotelDto { Name = name.Text, City = city.Text, Country = country.Text, Notes = notes.Text, IsActive = SelectedHotel.IsActive });
    }

    private async void NewReservation_Click(object sender, RoutedEventArgs e)
    {
        ComboBox customer = new() { Header = "Cliente", ItemsSource = ViewModel.Customers, DisplayMemberPath = "DisplayName", SelectedIndex = 0 }; ComboBox sale = new() { Header = "Venta", ItemsSource = ViewModel.Sales, DisplayMemberPath = "SaleNumber", SelectedIndex = 0 }; ComboBox hotel = new() { Header = "Hotel", ItemsSource = ViewModel.Hotels, DisplayMemberPath = "Name", SelectedIndex = 0 }; TextBox room = new() { PlaceholderText = "Tipo de habitación" }; TextBox occupancy = new() { PlaceholderText = "Personas" }; TextBox rooms = new() { PlaceholderText = "Habitaciones" }; TextBox cost = new() { PlaceholderText = "Costo" }; TextBox price = new() { PlaceholderText = "Precio de venta" }; DatePicker checkIn = new() { Date = DateTimeOffset.Now }; DatePicker checkOut = new() { Date = DateTimeOffset.Now.AddDays(1) }; TextBox notes = new() { PlaceholderText = "Notas" };
        StackPanel panel = new() { Spacing = 6, Children = { customer, sale, hotel, checkIn, checkOut, room, occupancy, rooms, cost, price, notes } };
        ContentDialog dialog = new() { Title = "Nueva reserva", Content = panel, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary || customer.SelectedItem is not CustomerDto customerDto || sale.SelectedItem is not SaleDto saleDto || hotel.SelectedItem is not HotelDto hotelDto) return;
        CreateHotelReservationDto dto = new() { CustomerId = customerDto.Id, SaleId = saleDto.Id, HotelId = hotelDto.Id, CheckIn = checkIn.Date.DateTime, CheckOut = checkOut.Date.DateTime, RoomType = room.Text, Occupancy = ParseInt(occupancy.Text), NumberOfRooms = ParseInt(rooms.Text), Cost = ParseDecimal(cost.Text), SalePrice = ParseDecimal(price.Text) };
        await ViewModel.SaveReservationAsync(null, dto, new UpdateHotelReservationDto());
    }

    private async void EditReservation_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedReservation is null) return;
        ComboBox status = new() { ItemsSource = Enum.GetValues<HotelReservationStatusEnum>(), SelectedItem = SelectedReservation.Status }; TextBox confirmation = new() { Text = SelectedReservation.ConfirmationNumber }; TextBox notes = new() { Text = SelectedReservation.Notes };
        ContentDialog dialog = new() { Title = "Editar reserva", Content = new StackPanel { Spacing = 8, Children = { status, confirmation, notes } }, PrimaryButtonText = "Guardar", CloseButtonText = "Cancelar", XamlRoot = XamlRoot };
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) await ViewModel.SaveReservationAsync(SelectedReservation, new CreateHotelReservationDto(), new UpdateHotelReservationDto { CheckIn = SelectedReservation.CheckIn, CheckOut = SelectedReservation.CheckOut, RoomType = SelectedReservation.RoomType, Occupancy = SelectedReservation.Occupancy, NumberOfRooms = SelectedReservation.NumberOfRooms, Cost = SelectedReservation.Cost, SalePrice = SelectedReservation.SalePrice, Currency = SelectedReservation.Currency, Status = (HotelReservationStatusEnum)status.SelectedItem, ConfirmationNumber = confirmation.Text, Notes = notes.Text });
    }

    private static int ParseInt(string value) => int.TryParse(value, out int result) ? result : 0;
    private static decimal ParseDecimal(string value) => decimal.TryParse(value, out decimal result) ? result : 0m;
}
