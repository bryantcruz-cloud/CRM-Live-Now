using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

/// <summary>Registers cancellations (including injuries) and updates slot states when applicable.</summary>
[ApiController]
[Route("api/cancellations")]
[Produces("application/json")]
public class CancellationsController : ApiControllerBase
{
    private readonly ICancellationService _cancellationService;

    public CancellationsController(ICancellationService cancellationService)
    {
        _cancellationService = cancellationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CancellationDto>>> GetAll(CancellationToken cancellationToken = default)
        => Ok(await _cancellationService.GetAllAsync(cancellationToken));

    /// <summary>
    /// Registers a cancellation inside a transaction. If the cancellation is
    /// reassignable the affected slot becomes Transferable (never auto-reassigned).
    /// Audits every change.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CancellationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CancellationDto>> Create(CreateCancellationDto dto, CancellationToken cancellationToken = default)
    {
        CancellationDto created = await _cancellationService.CreateAsync(dto, cancellationToken);
        return Created(string.Empty, created);
    }
}
