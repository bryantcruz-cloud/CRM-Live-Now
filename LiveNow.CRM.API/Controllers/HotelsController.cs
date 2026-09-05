using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/hotels")]
[Produces("application/json")]
public class HotelsController : ApiControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HotelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        return Ok(await _hotelService.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<HotelDto>> CreateHotel(CreateHotelDto dto, CancellationToken cancellationToken = default)
    {
        HotelDto created = await _hotelService.CreateAsync(dto, cancellationToken);
        return Created(string.Empty, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelDto>> UpdateHotel(Guid id, UpdateHotelDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _hotelService.UpdateAsync(id, dto, cancellationToken));
    }
}

[ApiController]
[Route("api/hotel-reservations")]
[Produces("application/json")]
public class HotelReservationsController : ApiControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelReservationsController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HotelReservationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HotelReservationDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        return Ok(await _hotelService.GetReservationsAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HotelReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HotelReservationDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _hotelService.GetReservationByIdAsync(id, cancellationToken));
    }

    /// <summary>Creates a reservation. Validates CheckOut &gt; CheckIn and consistent nights.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(HotelReservationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<HotelReservationDto>> Create(CreateHotelReservationDto dto, CancellationToken cancellationToken = default)
    {
        HotelReservationDto created = await _hotelService.CreateReservationAsync(dto, cancellationToken);
        return Created(string.Empty, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HotelReservationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<HotelReservationDto>> Update(Guid id, UpdateHotelReservationDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _hotelService.UpdateReservationAsync(id, dto, cancellationToken));
    }
}
