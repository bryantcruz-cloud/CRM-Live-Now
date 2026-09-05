using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/race-editions")]
public class RaceEditionsController : ApiControllerBase
{
    private readonly IRaceEditionService _raceEditionService;
    private readonly IRaceSlotService _raceSlotService;

    public RaceEditionsController(IRaceEditionService raceEditionService, IRaceSlotService raceSlotService)
    {
        _raceEditionService = raceEditionService;
        _raceSlotService = raceSlotService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RaceEditionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceEditionDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceEditionService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RaceEditionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceEditionDto>> Update(Guid id, UpdateRaceEditionDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceEditionService.UpdateAsync(id, dto, cancellationToken));
    }

    /// <summary>Paged slot inventory of an edition, optionally filtered by status.</summary>
    [HttpGet("{id:guid}/slots")]
    [ProducesResponseType(typeof(PagedResult<RaceSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<RaceSlotDto>>> GetSlots(
        Guid id,
        [FromQuery] SlotStatusEnum? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _raceSlotService.GetByEditionAsync(id, page, pageSize, status, cancellationToken));
    }

    /// <summary>Per-status slot counts for an edition (Total, Available, Reserved, Sold, ...).</summary>
    [HttpGet("{id:guid}/inventory-summary")]
    [ProducesResponseType(typeof(InventorySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InventorySummaryDto>> GetInventorySummary(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceSlotService.GetInventorySummaryAsync(id, cancellationToken));
    }
}
