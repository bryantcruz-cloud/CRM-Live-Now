using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/race-slots")]
[Produces("application/json")]
public class RaceSlotsController : ApiControllerBase
{
    private readonly IRaceSlotService _raceSlotService;
    private readonly ITransferService _transferService;

    public RaceSlotsController(IRaceSlotService raceSlotService, ITransferService transferService)
    {
        _raceSlotService = raceSlotService;
        _transferService = transferService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RaceSlotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RaceSlotDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceSlotService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(RaceSlotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RaceSlotDto>> Create(CreateRaceSlotDto dto, CancellationToken cancellationToken = default)
    {
        RaceSlotDto created = await _raceSlotService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a slot. Invalid state transitions are rejected (HTTP 409).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RaceSlotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RaceSlotDto>> Update(Guid id, UpdateRaceSlotDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceSlotService.UpdateAsync(id, dto, cancellationToken));
    }

    /// <summary>
    /// Inventory summary for an edition: total and per-status counts.
    /// </summary>
    [HttpGet("editions/{editionId:guid}/inventory-summary")]
    [ProducesResponseType(typeof(InventorySummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventorySummaryDto>> GetInventorySummary(Guid editionId, CancellationToken cancellationToken = default)
    {
        return Ok(await _raceSlotService.GetInventorySummaryAsync(editionId, cancellationToken));
    }

    /// <summary>
    /// Paged slots of an edition, optionally filtered by status.
    /// </summary>
    [HttpGet("editions/{editionId:guid}/slots")]
    [ProducesResponseType(typeof(PagedResult<RaceSlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RaceSlotDto>>> GetSlots(
        Guid editionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SlotStatusEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _raceSlotService.GetByEditionAsync(editionId, page, pageSize, status, cancellationToken));
    }

    /// <summary>
    /// Transfers the slot to another customer. A new SlotTransfer row is appended
    /// (history is never deleted). Sold/Registered slots must be cancelled as
    /// reassignable first (HTTP 409 otherwise).
    /// </summary>
    [HttpPost("{slotId:guid}/transfer")]
    [ProducesResponseType(typeof(SlotTransferDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<SlotTransferDto>> Transfer(Guid slotId, SlotTransferRequestDto dto, CancellationToken cancellationToken = default)
    {
        SlotTransferDto created = await _transferService.TransferSlotAsync(slotId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = slotId }, created);
    }
}
