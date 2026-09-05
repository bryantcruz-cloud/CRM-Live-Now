using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/quotes")]
[Produces("application/json")]
public class QuotesController : ApiControllerBase
{
    private readonly IQuoteService _quoteService;

    public QuotesController(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    /// <summary>Paged quote list.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<QuoteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<QuoteDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.GetAllAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Creates a quote as Draft. Totals (Subtotal/Discount/Taxes/Total) are calculated server-side.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteDto>> Create(CreateQuoteDto dto, CancellationToken cancellationToken = default)
    {
        QuoteDto created = await _quoteService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Replaces items and totals of a Draft or Sent quote.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteDto>> Update(Guid id, UpdateQuoteDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.UpdateAsync(id, dto, cancellationToken));
    }

    /// <summary>Draft to Sent.</summary>
    [HttpPost("{id:guid}/send")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteDto>> Send(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.SendAsync(id, cancellationToken));
    }

    /// <summary>Sent to Accepted.</summary>
    [HttpPost("{id:guid}/accept")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteDto>> Accept(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.AcceptAsync(id, cancellationToken));
    }

    /// <summary>Draft/Sent to Rejected.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuoteDto>> Cancel(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.CancelAsync(id, cancellationToken));
    }

    /// <summary>Converts an Accepted quote to a sale with slot assignments.</summary>
    [HttpPost("{id:guid}/convert-to-sale")]
    [ProducesResponseType(typeof(QuoteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<QuoteDto>> ConvertToSale(Guid id, ConvertQuoteToSaleDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _quoteService.ConvertToSaleAsync(id, dto, cancellationToken));
    }
}
