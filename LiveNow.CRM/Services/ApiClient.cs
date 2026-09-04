using System.Net.Http.Json;
using System.Text.Json;
using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Services;

/// <summary>
/// HTTP client wrapper for consuming LiveNow.CRM.API.
/// Handles serialization, error handling and provides typed methods for each endpoint.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl.TrimEnd('/'))
        };
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    // Customers
    public async Task<PagedResult<CustomerDto>?> GetCustomersAsync(string? search = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        string url = $"api/customers?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
        {
            url += $"&search={Uri.EscapeDataString(search)}";
        }
        return await GetAsync<PagedResult<CustomerDto>>(url, ct);
    }

    public async Task<CustomerDto?> GetCustomerAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<CustomerDto>($"api/customers/{id}", ct);
    }

    public async Task<CustomerDto?> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateCustomerDto, CustomerDto>("api/customers", dto, ct);
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default)
    {
        return await PutAsync<UpdateCustomerDto, CustomerDto>($"api/customers/{id}", dto, ct);
    }

    public async Task DeleteCustomerAsync(Guid id, CancellationToken ct = default)
    {
        HttpResponseMessage response = await _httpClient.DeleteAsync($"api/customers/{id}", ct);
        if (!response.IsSuccessStatusCode)
        {
            string errorBody = await response.Content.ReadAsStringAsync();
            throw new ApiException(response.StatusCode, errorBody);
        }
    }

    // Races
    public async Task<IReadOnlyList<RaceDto>?> GetRacesAsync(CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<RaceDto>>("api/races", ct);
    }

    public async Task<RaceDto?> CreateRaceAsync(CreateRaceDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateRaceDto, RaceDto>("api/races", dto, ct);
    }

    // Race Editions
    public async Task<IReadOnlyList<RaceEditionDto>?> GetEditionsAsync(Guid raceId, CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<RaceEditionDto>>($"api/races/{raceId}/editions", ct);
    }

    public async Task<RaceEditionDto?> CreateEditionAsync(Guid raceId, CreateRaceEditionDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateRaceEditionDto, RaceEditionDto>($"api/races/{raceId}/editions", dto, ct);
    }

    // Race Slots / Inventory
    public async Task<PagedResult<RaceSlotDto>?> GetSlotsAsync(Guid editionId, int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<RaceSlotDto>>($"api/race-editions/{editionId}/slots?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<InventorySummaryDto?> GetInventorySummaryAsync(Guid editionId, CancellationToken ct = default)
    {
        return await GetAsync<InventorySummaryDto>($"api/race-editions/{editionId}/inventory-summary", ct);
    }

    public async Task<RaceSlotDto?> CreateSlotAsync(CreateRaceSlotDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateRaceSlotDto, RaceSlotDto>("api/race-slots", dto, ct);
    }

    // Quotes
    public async Task<PagedResult<QuoteDto>?> GetQuotesAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<QuoteDto>>($"api/quotes?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<QuoteDto?> CreateQuoteAsync(CreateQuoteDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateQuoteDto, QuoteDto>("api/quotes", dto, ct);
    }

    public async Task<QuoteDto?> SendQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostAsync<QuoteDto>($"api/quotes/{id}/send", ct);
    }

    public async Task<QuoteDto?> AcceptQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostAsync<QuoteDto>($"api/quotes/{id}/accept", ct);
    }

    public async Task<QuoteDto?> CancelQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostAsync<QuoteDto>($"api/quotes/{id}/cancel", ct);
    }

    // Sales
    public async Task<PagedResult<SaleDto>?> GetSalesAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await GetAsync<PagedResult<SaleDto>>($"api/sales?page={page}&pageSize={pageSize}", ct);
    }

    public async Task<SaleDto?> GetSaleAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<SaleDto>($"api/sales/{id}", ct);
    }

    public async Task<SaleDto?> CreateSaleAsync(CreateSaleDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateSaleDto, SaleDto>("api/sales", dto, ct);
    }

    public async Task<SaleDto?> ConfirmSaleAsync(Guid id, ConfirmSaleDto dto, CancellationToken ct = default)
    {
        return await PostAsync<ConfirmSaleDto, SaleDto>($"api/sales/{id}/confirm", dto, ct);
    }

    // Payments
    public async Task<IReadOnlyList<PaymentDto>?> GetPaymentsAsync(Guid saleId, CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<PaymentDto>>($"api/sales/{saleId}/payments", ct);
    }

    public async Task<PaymentDto?> CreatePaymentAsync(Guid saleId, CreatePaymentDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreatePaymentDto, PaymentDto>($"api/sales/{saleId}/payments", dto, ct);
    }

    public async Task<SaleFinancialSummaryDto?> GetFinancialSummaryAsync(Guid saleId, CancellationToken ct = default)
    {
        return await GetAsync<SaleFinancialSummaryDto>($"api/reports/sales/{saleId}/financial-summary", ct);
    }

    // Hotels
    public async Task<IReadOnlyList<HotelDto>?> GetHotelsAsync(CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<HotelDto>>("api/hotels", ct);
    }

    public async Task<IReadOnlyList<HotelReservationDto>?> GetHotelReservationsAsync(CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<HotelReservationDto>>("api/hotel-reservations", ct);
    }

    // Registrations
    public async Task<IReadOnlyList<RegistrationDto>?> GetRegistrationsAsync(CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<RegistrationDto>>("api/registrations", ct);
    }

    // Cancellations
    public async Task<CancellationDto?> CreateCancellationAsync(CreateCancellationDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateCancellationDto, CancellationDto>("api/cancellations", dto, ct);
    }

    // Transfers
    public async Task<SlotTransferDto?> TransferSlotAsync(Guid slotId, SlotTransferRequestDto dto, CancellationToken ct = default)
    {
        return await PostAsync<SlotTransferRequestDto, SlotTransferDto>($"api/race-slots/{slotId}/transfer", dto, ct);
    }

    // Quotes
    public async Task<PagedResult<QuoteDto>?> GetQuotesAsync(int page = 1, int pageSize = 20, QuoteStatusEnum? status = null, CancellationToken ct = default)
    {
        string url = $"api/quotes?page={page}&pageSize={pageSize}";
        if (status.HasValue)
        {
            url += $"&status={(int)status.Value}";
        }
        return await GetAsync<PagedResult<QuoteDto>>(url, ct);
    }

    public async Task<QuoteDto?> GetQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await GetAsync<QuoteDto>($"api/quotes/{id}", ct);
    }

    public async Task<QuoteDto?> CreateQuoteAsync(CreateQuoteDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CreateQuoteDto, QuoteDto>("api/quotes", dto, ct);
    }

    public async Task<QuoteDto?> UpdateQuoteAsync(Guid id, UpdateQuoteDto dto, CancellationToken ct = default)
    {
        return await PutAsync<UpdateQuoteDto, QuoteDto>($"api/quotes/{id}", dto, ct);
    }

    public async Task<QuoteDto?> SendQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostAsync<QuoteDto>($"api/quotes/{id}/send", ct);
    }

    public async Task<QuoteDto?> AcceptQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostAsync<QuoteDto>($"api/quotes/{id}/accept", ct);
    }

    public async Task<QuoteDto?> CancelQuoteAsync(Guid id, CancellationToken ct = default)
    {
        return await PostAsync<QuoteDto>($"api/quotes/{id}/cancel", ct);
    }

    // Audit
    public async Task<IReadOnlyList<AuditLogDto>?> GetAuditLogsAsync(int limit = 50, CancellationToken ct = default)
    {
        return await GetAsync<IReadOnlyList<AuditLogDto>>($"api/reports/audit-logs?limit={limit}", ct);
    }

    // Health
    public async Task<bool> IsHealthyAsync(CancellationToken ct = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/health", ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Private helpers
    private async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest dto, CancellationToken ct)
    {
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, dto, JsonOptions, ct);
        return await HandleResponse<TResponse>(response);
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(url, ct);
        return await HandleResponse<T>(response);
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest dto, CancellationToken ct)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, dto, JsonOptions, ct);
        return await HandleResponse<TResponse>(response);
    }

    private async Task<T?> PostAsync<T>(string url, CancellationToken ct)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(url, null, ct);
        return await HandleResponse<T>(response);
    }

    private static async Task<T?> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }

        string errorBody = await response.Content.ReadAsStringAsync();
        throw new ApiException(response.StatusCode, errorBody);
    }
}

public class ApiException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }
    public string ErrorBody { get; }

    public ApiException(System.Net.HttpStatusCode statusCode, string errorBody)
        : base($"API error: {(int)statusCode} {statusCode}")
    {
        StatusCode = statusCode;
        ErrorBody = errorBody;
    }
}
