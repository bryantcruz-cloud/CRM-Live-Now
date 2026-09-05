using LiveNow.CRM.Core.DTOs;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IHotelService
{
    Task<IReadOnlyList<HotelDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<HotelDto> CreateAsync(CreateHotelDto dto, CancellationToken cancellationToken = default);

    Task<HotelDto> UpdateAsync(Guid id, UpdateHotelDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HotelReservationDto>> GetReservationsAsync(CancellationToken cancellationToken = default);

    Task<HotelReservationDto> GetReservationByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<HotelReservationDto> CreateReservationAsync(CreateHotelReservationDto dto, CancellationToken cancellationToken = default);

    Task<HotelReservationDto> UpdateReservationAsync(Guid id, UpdateHotelReservationDto dto, CancellationToken cancellationToken = default);
}