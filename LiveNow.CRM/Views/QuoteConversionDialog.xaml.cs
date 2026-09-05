using LiveNow.CRM.Core.DTOs;
using Microsoft.UI.Xaml.Controls;

namespace LiveNow.CRM.Views;

public sealed partial class QuoteConversionDialog : ContentDialog
{
    private readonly QuoteDto _quote;

    public QuoteConversionDialog(QuoteDto quote, IReadOnlyList<RaceSlotDto> slots)
    {
        InitializeComponent();
        _quote = quote;
        CustomerText.Text = $"Cliente: {quote.CustomerId}";
        QuoteText.Text = $"CotizaciÃ³n: {quote.QuoteNumber}";
        TotalText.Text = $"Total: {quote.Total:N2} {quote.Currency}";
        SlotsListView.ItemsSource = slots;
    }

    public ConvertQuoteToSaleDto GetConversionDto()
    {
        return new ConvertQuoteToSaleDto
        {
            SlotAssignments = SlotsListView.SelectedItems
                .OfType<RaceSlotDto>()
                .Select(slot => new ConvertSlotAssignmentDto
                {
                    SlotId = slot.Id,
                    UnitPrice = _quote.Items.FirstOrDefault(item => item.ItemType == Core.Enums.QuoteItemTypeEnum.Entry)?.UnitPrice ?? 0m
                })
                .ToList()
        };
    }
}

