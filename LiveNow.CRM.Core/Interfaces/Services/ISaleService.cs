using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface ISaleService
{
    Task<PagedResult<SaleDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<SaleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SaleDto> CreateAsync(CreateSaleDto dto, CancellationToken cancellationToken = default);

    Task<SaleDto> UpdateAsync(Guid id, UpdateSaleDto dto, CancellationToken cancellationToken = default);

    Task<SaleDto> ConfirmAsync(Guid id, ConfirmSaleDto dto, CancellationToken cancellationToken = default);
}