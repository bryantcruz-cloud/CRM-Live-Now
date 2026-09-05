using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/races")]
[Produces("application/json")]
public class RacesController : ApiControllerBase
{
    private readonly IRaceService _raceService;

    public RacesController(IRaceService raceService)
    {
        _raceService = raceService;
    }

    /// <summary>All races (Chicago Marathon, Berlin Marathon, ...). Races are data, never code.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RaceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RaceDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        return Ok(await _raceService.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RaceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RaceDto>> Create(CreateRaceDto dto, CancellationToken cancellationToken = default)
    {
        RaceDto created = await _raceService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RaceDto>> Update(Guid id, UpdateRaceDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceService.UpdateAsync(id, dto, cancellationToken));
    }

    /// <summary>Editions of a race (e.g. Chicago Marathon 2026 / 2027).</summary>
    [HttpGet("{raceId:guid}/editions")]
    [ProducesResponseType(typeof(IReadOnlyList<RaceEditionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RaceEditionDto>>> GetEditions(Guid raceId, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceService.GetEditionsAsync(raceId, cancellationToken));
    }

    /// <summary>Creates a new edition. Rejects a duplicated Race + Year combination (HTTP 409).</summary>
    [HttpPost("{raceId:guid}/editions")]
    [ProducesResponseType(typeof(RaceEditionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RaceEditionDto>> CreateEdition(Guid raceId, CreateRaceEditionDto dto, CancellationToken cancellationToken = default)
    {
        RaceEditionDto created = await _raceService.CreateEditionAsync(raceId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetEditions), new { raceId }, created);
    }
}


