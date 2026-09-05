using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IQuoteService
{
    Task<PagedResult<QuoteDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<QuoteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuoteDto> CreateAsync(CreateQuoteDto dto, CancellationToken cancellationToken = default);

    Task<QuoteDto> UpdateAsync(Guid id, UpdateQuoteDto dto, CancellationToken cancellationToken = default);

    Task<QuoteDto> SendAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuoteDto> AcceptAsync(Guid id, CancellationToken cancellationToken = default);

    Task<QuoteDto> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts an Accepted quote to a sale. Creates the sale, confirms it with slot assignments,
    /// and updates the quote status to ConvertedToSale. All within a transaction.
    /// </summary>
    Task<QuoteDto> ConvertToSaleAsync(Guid id, ConvertQuoteToSaleDto dto, CancellationToken cancellationToken = default);
}