using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IRaceService
{
    Task<IReadOnlyList<RaceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<RaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RaceDto> CreateAsync(CreateRaceDto dto, CancellationToken cancellationToken = default);

    Task<RaceDto> UpdateAsync(Guid id, UpdateRaceDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RaceEditionDto>> GetEditionsAsync(Guid raceId, CancellationToken cancellationToken = default);

    Task<RaceEditionDto> CreateEditionAsync(Guid raceId, CreateRaceEditionDto dto, CancellationToken cancellationToken = default);

    Task<RaceEditionDto> GetEditionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RaceEditionDto> UpdateEditionAsync(Guid id, UpdateRaceEditionDto dto, CancellationToken cancellationToken = default);
}

public interface IRaceEditionService
{
    Task<RaceEditionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<RaceEditionDto> UpdateAsync(Guid id, UpdateRaceEditionDto dto, CancellationToken cancellationToken = default);
}