using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IPaymentService
{
    Task<PagedResult<PaymentDto>> GetBySaleAsync(
        Guid saleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> RegisterPaymentAsync(
        Guid saleId,
        CreatePaymentDto dto,
        CancellationToken cancellationToken = default);

    Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto dto, CancellationToken cancellationToken = default);

    Task<SaleFinancialSummaryDto> GetFinancialSummaryAsync(Guid saleId, CancellationToken cancellationToken = default);
}