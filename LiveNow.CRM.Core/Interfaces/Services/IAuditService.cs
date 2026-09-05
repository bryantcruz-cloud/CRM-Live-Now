using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Interfaces.Services;

public interface IAuditService
{
    Task RecordAsync(
        string entityName,
        string entityId,
        AuditActionEnum action,
        string? oldValues = null,
        string? newValues = null,
        string? userId = null,
        CancellationToken cancellationToken = default);
}