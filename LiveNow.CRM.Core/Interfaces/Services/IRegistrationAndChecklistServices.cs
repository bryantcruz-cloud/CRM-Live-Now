using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IChecklistService
{
    Task<IReadOnlyList<ChecklistItemDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<ChecklistItemDto> CreateAsync(Guid customerId, CreateChecklistItemDto dto, CancellationToken cancellationToken = default);

    Task<ChecklistItemDto> UpdateAsync(Guid id, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default);
}

public interface ICancellationService
{
    Task<CancellationDto> CreateAsync(CreateCancellationDto dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CancellationDto>> GetAllAsync(CancellationToken cancellationToken = default);
}

