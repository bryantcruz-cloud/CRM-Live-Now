using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/reports")]
[Produces("application/json")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>Most recent audit log entries (for the audit view).</summary>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetAuditLogs(
        [FromQuery] int limit = 50,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? entity = null,
        [FromQuery] AuditActionEnum? action = null,
        [FromQuery] string? userId = null,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _reportingService.GetRecentAuditLogsAsync(limit, from, to, entity, action, userId, cancellationToken));
    }

    [HttpGet("operational-summary")]
    public async Task<ActionResult<OperationalSummaryDto>> GetOperationalSummary(CancellationToken cancellationToken = default)
        => Ok(await _reportingService.GetOperationalSummaryAsync(cancellationToken));
}
