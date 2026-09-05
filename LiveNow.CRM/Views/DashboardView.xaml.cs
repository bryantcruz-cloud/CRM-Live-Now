using System.Collections.ObjectModel;
using LiveNow.CRM.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class DashboardView : Page
{
    public DashboardViewModel ViewModel { get; private set; }

    public ObservableCollection<SummaryCard> SummaryCards { get; } = new();

    public DashboardView(DashboardViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateSummaryCards();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DashboardViewModel.TotalCustomers) or
            nameof(DashboardViewModel.TotalQuotes) or
            nameof(DashboardViewModel.TotalSales) or
            nameof(DashboardViewModel.PendingPayments) or
            nameof(DashboardViewModel.AvailableSlots))
        {
            UpdateSummaryCards();
        }
    }

    private void UpdateSummaryCards()
    {
        SummaryCards.Clear();
        SummaryCards.Add(new SummaryCard { Title = "Clientes", Value = ViewModel.TotalCustomers.ToString(), Subtitle = "Total registrados" });
        SummaryCards.Add(new SummaryCard { Title = "Cotizaciones", Value = ViewModel.TotalQuotes.ToString(), Subtitle = "Cotizaciones activas" });
        SummaryCards.Add(new SummaryCard { Title = "Ventas", Value = ViewModel.TotalSales.ToString(), Subtitle = "Ventas confirmadas" });
        SummaryCards.Add(new SummaryCard { Title = "Pagos Pendientes", Value = ViewModel.PendingPayments.ToString(), Subtitle = "Requieren atenciÃ³n" });
        SummaryCards.Add(new SummaryCard { Title = "Plazas Disponibles", Value = ViewModel.AvailableSlots.ToString(), Subtitle = "En inventario" });
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadDashboardDataAsync();
    }
}

public class SummaryCard
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
}

