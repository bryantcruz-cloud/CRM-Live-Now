using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;

namespace LiveNow.CRM.API.Services;

public class AuditService : IAuditService
{
    private readonly LiveNowDbContext _context;

    public AuditService(LiveNowDbContext context)
    {
        _context = context;
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
                UserId = userId,
                Timestamp = DateTime.UtcNow
            },
            cancellationToken);
    }
}