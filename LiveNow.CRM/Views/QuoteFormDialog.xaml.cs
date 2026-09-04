using System.Collections.ObjectModel;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class QuoteFormDialog : ContentDialog
{
    private readonly ObservableCollection<CustomerDto> _customers;
    private readonly ObservableCollection<RaceDto> _races;
    private readonly ObservableCollection<RaceEditionDto> _editions;
    private readonly ObservableCollection<QuoteItemEntry> _items = new();
    private readonly QuoteDto? _existingQuote;

    private decimal _discount;
    private decimal _taxes;

    public string SubtotalText => _items.Sum(i => i.TotalPrice).ToString("N2");
    public string DiscountText => _discount.ToString("N2");
    public string TaxesText => _taxes.ToString("N2");
    public string TotalText => (_items.Sum(i => i.TotalPrice) - _discount + _taxes).ToString("N2");

    public QuoteFormDialog(
        ObservableCollection<CustomerDto> customers,
        ObservableCollection<RaceDto> races,
        ObservableCollection<RaceEditionDto> editions,
        QuoteDto? existingQuote = null)
    {
        this.InitializeComponent();
        _customers = customers;
        _races = races;
        _editions = editions;
        _existingQuote = existingQuote;

        Title = existingQuote != null ? $"Editar cotización #{existingQuote.QuoteNumber}" : "Nueva cotización";

        CustomerComboBox.ItemsSource = _customers;
        RaceComboBox.ItemsSource = _races;
        EditionComboBox.ItemsSource = _editions;

        CurrencyComboBox.ItemsSource = Enum.GetValues(typeof(CurrencyEnum));
        CurrencyComboBox.SelectedItem = CurrencyEnum.USD;

        if (existingQuote != null)
        {
            LoadExistingData(existingQuote);
        }

        UpdateTotals();
    }

    private void LoadExistingData(QuoteDto quote)
    {
        CustomerComboBox.SelectedItem = _customers.FirstOrDefault(c => c.Id == quote.CustomerId);
        CurrencyComboBox.SelectedItem = quote.Currency;
        _discount = quote.Discount;
        _taxes = quote.Taxes;
        NotesTextBoxHeader.Text = quote.Notes ?? string.Empty;

        if (quote.ValidUntil.HasValue)
        {
            ValidUntilDatePicker.Date = new DateTimeOffset(quote.ValidUntil.Value);
        }

        foreach (var item in quote.Items)
        {
            _items.Add(new QuoteItemEntry
            {
                Description = item.Description,
                ItemType = item.ItemType,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                UnitPrice = item.UnitPrice
            });
        }
        RefreshItemsList();
    }

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        _items.Add(new QuoteItemEntry
        {
            ItemType = QuoteItemTypeEnum.Entry,
            Quantity = 1
        });
        RefreshItemsList();
        UpdateTotals();
    }

    private void RefreshItemsList()
    {
        ItemsPanel.Children.Clear();
        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            var panel = new StackPanel { Spacing = 4 };

            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var title = new TextBlock { Text = $"Item {i + 1}", FontWeight = FontWeights.SemiBold };
            var removeBtn = new Button { Content = "Eliminar", Tag = i };
            removeBtn.Click += RemoveItem_Click;

            Grid.SetColumn(title, 0);
            Grid.SetColumn(removeBtn, 1);
            header.Children.Add(title);
            header.Children.Add(removeBtn);

            panel.Children.Add(header);

            var typeCombo = new ComboBox
            {
                ItemsSource = Enum.GetValues(typeof(QuoteItemTypeEnum)),
                SelectedItem = item.ItemType,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            typeCombo.SelectionChanged += (s, e) => { item.ItemType = (QuoteItemTypeEnum)typeCombo.SelectedItem; };

            var descBox = new TextBox { Text = item.Description, PlaceholderText = "Descripción" };
            descBox.TextChanged += (s, e) => { item.Description = descBox.Text; };

            var pricePanel = new Grid();
            pricePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pricePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var costBox = new TextBox { Text = item.UnitCost.ToString(), PlaceholderText = "Costo unitario", Margin = new Thickness(0, 0, 4, 0) };
            costBox.TextChanged += (s, e) => { if (decimal.TryParse(costBox.Text, out decimal v)) item.UnitCost = v; UpdateTotals(); };

            var priceBox = new TextBox { Text = item.UnitPrice.ToString(), PlaceholderText = "Precio unitario", Margin = new Thickness(4, 0, 0, 0) };
            priceBox.TextChanged += (s, e) => { if (decimal.TryParse(priceBox.Text, out decimal v)) item.UnitPrice = v; UpdateTotals(); };

            Grid.SetColumn(costBox, 0);
            Grid.SetColumn(priceBox, 1);
            pricePanel.Children.Add(costBox);
            pricePanel.Children.Add(priceBox);

            panel.Children.Add(typeCombo);
            panel.Children.Add(descBox);
            panel.Children.Add(pricePanel);

            ItemsPanel.Children.Add(panel);
        }
    }

    private void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int index && index < _items.Count)
        {
            _items.RemoveAt(index);
            RefreshItemsList();
            UpdateTotals();
        }
    }

    private void UpdateTotals()
    {
        Bindings.Update();
    }

    protected override void OnContentDialogButtonClick(ContentDialogButtonClickEventArgs args)
    {
        if (args.Button == ContentDialogButton.Primary)
        {
            if (!ValidateForm())
            {
                args.Cancel = true;
                return;
            }
        }
        base.OnContentDialogButtonClick(args);
    }

    private bool ValidateForm()
    {
        if (CustomerComboBox.SelectedItem == null)
        {
            ShowValidationError("Debe seleccionar un cliente.");
            return false;
        }

        if (RaceComboBox.SelectedItem == null)
        {
            ShowValidationError("Debe seleccionar una carrera.");
            return false;
        }

        if (EditionComboBox.SelectedItem == null)
        {
            ShowValidationError("Debe seleccionar una edición.");
            return false;
        }

        if (_items.Count == 0)
        {
            ShowValidationError("Debe agregar al menos un item.");
            return false;
        }

        foreach (var item in _items)
        {
            if (string.IsNullOrWhiteSpace(item.Description))
            {
                ShowValidationError("Todos los items deben tener una descripción.");
                return false;
            }
        }

        ValidationInfoBar.IsOpen = false;
        return true;
    }

    private void ShowValidationError(string message)
    {
        ValidationInfoBar.Message = message;
        ValidationInfoBar.IsOpen = true;
    }

    public CreateQuoteDto? GetCreateDto()
    {
        if (!ValidateForm()) return null;

        return new CreateQuoteDto
        {
            CustomerId = ((CustomerDto)CustomerComboBox.SelectedItem).Id,
            RaceEditionId = ((RaceEditionDto)EditionComboBox.SelectedItem).Id,
            Currency = (CurrencyEnum)CurrencyComboBox.SelectedItem,
            ValidUntil = ValidUntilDatePicker.Date?.DateTime,
            Discount = _discount,
            Taxes = _taxes,
            Notes = string.IsNullOrWhiteSpace(NotesTextBoxHeader.Text) ? null : NotesTextBoxHeader.Text,
            Items = _items.Select(i => new CreateQuoteItemDto
            {
                Description = i.Description,
                ItemType = i.ItemType,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }

    public UpdateQuoteDto? GetUpdateDto()
    {
        if (!ValidateForm()) return null;

        return new UpdateQuoteDto
        {
            Currency = (CurrencyEnum)CurrencyComboBox.SelectedItem,
            ValidUntil = ValidUntilDatePicker.Date?.DateTime,
            Discount = _discount,
            Taxes = _taxes,
            Notes = string.IsNullOrWhiteSpace(NotesTextBoxHeader.Text) ? null : NotesTextBoxHeader.Text,
            Items = _items.Select(i => new CreateQuoteItemDto
            {
                Description = i.Description,
                ItemType = i.ItemType,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}

public class QuoteItemEntry
{
    public string Description { get; set; } = string.Empty;
    public QuoteItemTypeEnum ItemType { get; set; } = QuoteItemTypeEnum.Entry;
    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
