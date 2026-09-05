using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IReportingService
{
    Task<SaleFinancialSummaryDto> GetSaleFinancialSummaryAsync(Guid saleId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLogDto>> GetRecentAuditLogsAsync(int limit, DateTime? from = null, DateTime? to = null, string? entity = null, AuditActionEnum? action = null, string? userId = null, CancellationToken cancellationToken = default);

    Task<OperationalSummaryDto> GetOperationalSummaryAsync(CancellationToken cancellationToken = default);
}
