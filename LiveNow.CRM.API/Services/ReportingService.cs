using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

/// <summary>
/// Reporting queries. Financial figures are always recomputed from source data
/// (see <see cref="SaleFinancials"/> and <see cref="IFinancialCalculator"/>),
/// never trusted from previously stored aggregates.
/// </summary>
public class ReportingService : IReportingService
{
    private readonly LiveNowDbContext _context;
    private readonly IPaymentService _paymentService;

    public ReportingService(LiveNowDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public Task<SaleFinancialSummaryDto> GetSaleFinancialSummaryAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        return _paymentService.GetFinancialSummaryAsync(saleId, cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAuditLogsAsync(int limit, DateTime? from = null, DateTime? to = null, string? entity = null, AuditActionEnum? action = null, string? userId = null, CancellationToken cancellationToken = default)
    {
        int safeLimit = limit <= 0 ? 50 : Math.Min(limit, 500);

        IQueryable<AuditLog> query = _context.AuditLogs.AsNoTracking();
        if (from.HasValue)
        {
            query = query.Where(l => l.Timestamp >= from.Value);
        }
        if (to.HasValue)
        {
            query = query.Where(l => l.Timestamp < to.Value.Date.AddDays(1));
        }
        if (!string.IsNullOrWhiteSpace(entity))
        {
            query = query.Where(l => l.EntityName == entity);
        }
        if (action.HasValue)
        {
            query = query.Where(l => l.Action == action.Value);
        }
        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(l => l.UserId == userId);
        }

        List<AuditLogDto> logs = await query
            .OrderByDescending(l => l.Timestamp)
            .Take(safeLimit)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                EntityName = l.EntityName,
                EntityId = l.EntityId,
                Action = l.Action.ToString(),
                OldValues = l.OldValues,
                NewValues = l.NewValues,
                Timestamp = l.Timestamp
            })
            .ToListAsync(cancellationToken);

        return logs;
    }

    public async Task<OperationalSummaryDto> GetOperationalSummaryAsync(CancellationToken cancellationToken = default)
    {
        List<Sale> activeSales = await _context.Sales.Where(s => s.Status != SaleStatusEnum.Cancelled && s.Status != SaleStatusEnum.Refunded).AsNoTracking().ToListAsync(cancellationToken);
        List<Payment> validPayments = await _context.Payments.Where(p => p.Status == PaymentStatusEnum.Pending || p.Status == PaymentStatusEnum.Completed).AsNoTracking().ToListAsync(cancellationToken);
        decimal salesTotal = activeSales.Sum(s => s.TotalSalePrice);
        decimal paid = validPayments.Sum(p => p.Amount);
        return new OperationalSummaryDto
        {
            TotalCustomers = await _context.Customers.CountAsync(cancellationToken),
            TotalSales = await _context.Sales.CountAsync(cancellationToken),
            PendingPayments = await _context.Sales.CountAsync(s => s.Status == SaleStatusEnum.Pending || s.Status == SaleStatusEnum.PartiallyPaid, cancellationToken),
            PendingRegistrations = await _context.RunnerRegistrations.CountAsync(r => r.RegistrationStatus != RegistrationStatusEnum.Completed && r.RegistrationStatus != RegistrationStatusEnum.Cancelled, cancellationToken),
            AvailableSlots = await _context.RaceSlots.CountAsync(s => s.Status == SlotStatusEnum.Available, cancellationToken),
            ReservedSlots = await _context.RaceSlots.CountAsync(s => s.Status == SlotStatusEnum.Reserved, cancellationToken),
            SoldSlots = await _context.RaceSlots.CountAsync(s => s.Status == SlotStatusEnum.Sold || s.Status == SlotStatusEnum.Registered, cancellationToken),
            TransferableSlots = await _context.RaceSlots.CountAsync(s => s.Status == SlotStatusEnum.Transferable, cancellationToken),
            TotalCancellations = await _context.Cancellations.CountAsync(cancellationToken),
            InjuredCustomers = await _context.Cancellations.CountAsync(c => c.IsInjury, cancellationToken),
            Hotels = await _context.Hotels.CountAsync(h => h.IsActive, cancellationToken),
            HotelReservations = await _context.HotelReservations.CountAsync(cancellationToken),
            SalesTotal = salesTotal,
            PaidAmount = paid,
            OutstandingBalance = salesTotal - paid,
            GrossProfit = activeSales.Sum(s => s.GrossProfit)
        };
    }
}
