using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IRegistrationService
{
    Task<PagedResult<RegistrationDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<RegistrationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RegistrationDto> CreateAsync(CreateRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<RegistrationDto> UpdateAsync(Guid id, UpdateRegistrationDto dto, CancellationToken cancellationToken = default);
}
