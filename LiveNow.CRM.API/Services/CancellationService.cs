using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

/// <summary>
/// Registers cancellations. When the cancelled sale has race slots and
/// <see cref="CreateCancellationDto.CanReassignSlot"/> is true, those slots are
/// moved to Transferable so they can later be reassigned via TransferService
/// (never automatically to another customer). Everything runs in a transaction.
/// </summary>
public class CancellationService : ICancellationService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CancellationService(LiveNowDbContext context, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<CancellationDto> CreateAsync(CreateCancellationDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId, cancellationToken))
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ValidationException("CANCELLATION_REASON_REQUIRED", "El motivo de la cancelación es obligatorio.");
        }

        if (dto.RefundAmount is < 0m)
        {
            throw new ValidationException("CANCELLATION_REFUND_INVALID", "El monto de reembolso no puede ser negativo.");
        }

        Sale? sale = null;
        if (dto.SaleId is not null)
        {
            sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == dto.SaleId, cancellationToken)
                ?? throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");

            if (sale.CustomerId != dto.CustomerId)
            {
                throw new ConflictException("CANCELLATION_CUSTOMER_MISMATCH", "La venta pertenece a otro cliente.");
            }

            if (sale.Status is SaleStatusEnum.Cancelled or SaleStatusEnum.Refunded)
            {
                throw new ConflictException("SALE_ALREADY_CANCELLED", "La venta ya está cancelada.");
            }
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            Cancellation cancellation = new()
            {
                CustomerId = dto.CustomerId,
                SaleId = dto.SaleId,
                RaceSlotId = dto.RaceSlotId,
                CancellationDate = dto.CancellationDate,
                Reason = dto.Reason.Trim(),
                IsInjury = dto.IsInjury,
                CanReassignSlot = dto.CanReassignSlot,
                RefundAmount = dto.RefundAmount,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cancellations.Add(cancellation);

            List<RaceSlot> affectedSlots = await ResolveAffectedSlotsAsync(dto, sale, cancellationToken);

            foreach (RaceSlot slot in affectedSlots)
            {
                SlotStatusEnum previousStatus = slot.Status;

                if (dto.CanReassignSlot)
                {
                    // The slot keeps its history but becomes available for a
                    // future transfer; no automatic reassignment happens here.
                    slot.Status = SlotStatusEnum.Transferable;
                }
                else
                {
                    slot.Status = SlotStatusEnum.Cancelled;
                }

                slot.Version++;
                slot.UpdatedAt = DateTime.UtcNow;

                await _auditService.RecordAsync("RaceSlot", slot.Id.ToString(), AuditActionEnum.StatusChange, oldValues: previousStatus.ToString(), newValues: slot.Status.ToString(), userId: null, cancellationToken: cancellationToken);
            }

            if (sale is not null)
            {
                SaleStatusEnum previousSaleStatus = sale.Status;
                sale.Status = dto.RefundAmount is > 0m ? SaleStatusEnum.Refunded : SaleStatusEnum.Cancelled;
                sale.UpdatedAt = DateTime.UtcNow;

                await _auditService.RecordAsync("Sale", sale.Id.ToString(), AuditActionEnum.StatusChange, oldValues: previousSaleStatus.ToString(), newValues: sale.Status.ToString(), userId: null, cancellationToken: cancellationToken);
            }

            await _auditService.RecordAsync("Cancellation", cancellation.Id.ToString(), AuditActionEnum.Create, newValues: $"Customer={dto.CustomerId}; IsInjury={dto.IsInjury}; CanReassign={dto.CanReassignSlot}", userId: null, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return CancellationDto.FromEntity(cancellation);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<CancellationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Cancellation> items = await _context.Cancellations.AsNoTracking()
            .Include(c => c.Customer).Include(c => c.Sale).Include(c => c.RaceSlot)
            .OrderByDescending(c => c.CancellationDate).ToListAsync(cancellationToken);
        return items.Select(CancellationDto.FromEntity).ToList();
    }

    private async Task<List<RaceSlot>> ResolveAffectedSlotsAsync(CreateCancellationDto dto, Sale? sale, CancellationToken cancellationToken)
    {
        List<RaceSlot> slots = new();

        if (dto.RaceSlotId is not null)
        {
            RaceSlot? slot = await _context.RaceSlots.FirstOrDefaultAsync(s => s.Id == dto.RaceSlotId, cancellationToken)
                ?? throw new NotFoundException("RACE_SLOT_NOT_FOUND", "La plaza no existe.");

            slots.Add(slot);
        }
        else if (sale is not null)
        {
            var slotIds = sale.Items
                .Where(i => i.RaceSlotId is not null)
                .Select(i => i.RaceSlotId!.Value)
                .Distinct()
                .ToList();

            if (slotIds.Count > 0)
            {
                slots = await _context.RaceSlots
                    .Where(s => slotIds.Contains(s.Id))
                    .ToListAsync(cancellationToken);
            }
        }

        return slots;
    }
}
