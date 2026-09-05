using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IRaceSlotService
{
    Task<PagedResult<RaceSlotDto>> GetByEditionAsync(
        Guid raceEditionId,
        int page,
        int pageSize,
        SlotStatusEnum? status,
        CancellationToken cancellationToken = default);

    Task<RaceSlotDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RaceSlotDto> CreateAsync(CreateRaceSlotDto dto, CancellationToken cancellationToken = default);

    Task<RaceSlotDto> UpdateAsync(Guid id, UpdateRaceSlotDto dto, CancellationToken cancellationToken = default);

    Task<InventorySummaryDto> GetInventorySummaryAsync(Guid raceEditionId, CancellationToken cancellationToken = default);
}