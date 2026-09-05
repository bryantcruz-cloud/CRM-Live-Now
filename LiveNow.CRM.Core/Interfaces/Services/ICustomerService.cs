using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);

    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}