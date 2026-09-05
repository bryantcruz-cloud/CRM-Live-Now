using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class FinancialReportsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public FinancialReportsController(IPaymentService paymentService) => _paymentService = paymentService;

    [HttpGet("sales/{saleId:guid}/financial-summary")]
    public async Task<ActionResult<SaleFinancialSummaryDto>> GetSaleFinancialSummary(Guid saleId, CancellationToken cancellationToken = default)
        => Ok(await _paymentService.GetFinancialSummaryAsync(saleId, cancellationToken));
}
