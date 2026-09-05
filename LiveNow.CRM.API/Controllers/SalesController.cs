using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/sales")]
[Produces("application/json")]
public class SalesController : ApiControllerBase
{
    private readonly ISaleService _saleService;
    private readonly IPaymentService _paymentService;

    public SalesController(ISaleService saleService, IPaymentService paymentService)
    {
        _saleService = saleService;
        _paymentService = paymentService;
    }

    /// <summary>Paged sale list.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SaleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SaleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _saleService.GetAllAsync(page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _saleService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>Creates a Pending sale with its items.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<SaleDto>> Create(CreateSaleDto dto, CancellationToken cancellationToken = default)
    {
        SaleDto created = await _saleService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<SaleDto>> Update(Guid id, UpdateSaleDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _saleService.UpdateAsync(id, dto, cancellationToken));
    }

    /// <summary>
    /// Confirms a Pending sale inside a transaction: validates customer/edition,
    /// assigns and reserves the race slots (state Available/Reserved/Transferable only),
    /// recalculates all financial figures and audits everything. Any failure rolls back.
    /// Concurrent confirmations of the same slot are rejected via optimistic concurrency (HTTP 409).
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<SaleDto>> Confirm(Guid id, ConfirmSaleDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _saleService.ConfirmAsync(id, dto, cancellationToken));
    }

    /// <summary>Paged payments of the sale.</summary>
    [HttpGet("{saleId:guid}/payments")]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetPayments(
        Guid saleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _paymentService.GetBySaleAsync(saleId, page, pageSize, cancellationToken));
    }

    /// <summary>
    /// Registers a payment. Rejects negative/zero amounts and payments exceeding
    /// the outstanding balance (HTTP 422). Calculates the payment fee and
    /// recalculates all sale financials inside a transaction.
    /// </summary>
    [HttpPost("{saleId:guid}/payments")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PaymentDto>> RegisterPayment(Guid saleId, CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        PaymentDto created = await _paymentService.RegisterPaymentAsync(saleId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = saleId }, created);
    }

    /// <summary>Financial summary: totals, paid amount, outstanding balance, cost, fees, gross profit and margin.</summary>
    [HttpGet("{saleId:guid}/financial-summary")]
    [ProducesResponseType(typeof(SaleFinancialSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaleFinancialSummaryDto>> GetFinancialSummary(Guid saleId, CancellationToken cancellationToken = default)
    {
        return Ok(await _paymentService.GetFinancialSummaryAsync(saleId, cancellationToken));
    }
}

[ApiController]
[Route("api/payments")]
[Produces("application/json")]
public class PaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>Updates an existing payment and recalculates the sale financials.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PaymentDto>> Update(Guid id, UpdatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _paymentService.UpdateAsync(id, dto, cancellationToken));
    }
}
