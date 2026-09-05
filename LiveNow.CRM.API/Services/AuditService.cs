using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LiveNow.CRM.API.Services;

public class AuditService : IAuditService
{
    private readonly LiveNowDbContext _context;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AuditService(LiveNowDbContext context, IHttpContextAccessor? httpContextAccessor = null)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RecordAsync(
        string entityName,
        string entityId,
        AuditActionEnum action,
        string? oldValues = null,
        string? newValues = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        await _context.AuditLogs.AddAsync(
            new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = userId ?? _httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
                Timestamp = DateTime.UtcNow
            },
            cancellationToken);
    }
}
