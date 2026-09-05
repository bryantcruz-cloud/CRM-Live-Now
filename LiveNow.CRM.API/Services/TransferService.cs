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
/// Transfers a race slot from one customer to another.
/// The full history is preserved: a new SlotTransfer row is appended for every
/// transfer and previous rows are never deleted or modified.
/// Runs inside a transaction; slot changes are protected by the optimistic
/// concurrency token (RaceSlot.Version).
/// </summary>
public class TransferService : ITransferService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public TransferService(LiveNowDbContext context, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<SlotTransferDto> TransferSlotAsync(Guid slotId, SlotTransferRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ToCustomerId == Guid.Empty)
        {
            throw new ValidationException("TRANSFER_TARGET_REQUIRED", "Debe indicar el cliente destino.");
        }

        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            throw new ValidationException("TRANSFER_REASON_REQUIRED", "El motivo de la transferencia es obligatorio.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            RaceSlot? slot = await _context.RaceSlots.FirstOrDefaultAsync(s => s.Id == slotId, cancellationToken)
                ?? throw new NotFoundException("RACE_SLOT_NOT_FOUND", "La plaza no existe.");

            if (slot.Status is SlotStatusEnum.Sold or SlotStatusEnum.Registered)
            {
                // A sold/registered slot can only be transferred with a prior
                // cancellation marked as reassignable; block direct transfers.
                throw new ConflictException("RACE_SLOT_NOT_TRANSFERABLE", $"La plaza {slot.InternalCode} está {slot.Status}. Debe cancelarse marcándola como reasignable antes de transferirla.");
            }

            if (slot.Status is not (SlotStatusEnum.Transferable or SlotStatusEnum.Reserved))
            {
                throw new ConflictException("RACE_SLOT_NOT_TRANSFERABLE", $"La plaza {slot.InternalCode} no está en un estado transferible (estado actual: {slot.Status}).");
            }

            Customer? targetCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == dto.ToCustomerId, cancellationToken)
                ?? throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente destino no existe.");

            if (targetCustomer.Status != CustomerStatusEnum.Active)
            {
                throw new ConflictException("CUSTOMER_INACTIVE", "El cliente destino no está activo.");
            }

            Guid? fromCustomerId = slot.AssignedCustomerId;

            if (fromCustomerId == dto.ToCustomerId)
            {
                throw new ConflictException("TRANSFER_SAME_CUSTOMER", "El cliente destino ya es el asignado actual de la plaza.");
            }

            SlotTransfer transfer = new()
            {
                RaceSlotId = slot.Id,
                FromCustomerId = fromCustomerId,
                ToCustomerId = dto.ToCustomerId,
                TransferDate = DateTime.UtcNow,
                Reason = dto.Reason.Trim(),
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.SlotTransfers.Add(transfer);

            SlotStatusEnum previousStatus = slot.Status;
            slot.AssignedCustomerId = dto.ToCustomerId;
            slot.Status = SlotStatusEnum.Reserved;
            slot.ReservationDate = DateTime.UtcNow;
            slot.Version++;
            slot.UpdatedAt = DateTime.UtcNow;

            await _auditService.RecordAsync("SlotTransfer", transfer.Id.ToString(), AuditActionEnum.Create, newValues: $"Slot={slot.InternalCode}; To={dto.ToCustomerId}", userId: null, cancellationToken: cancellationToken);
            await _auditService.RecordAsync("RaceSlot", slot.Id.ToString(), AuditActionEnum.StatusChange, oldValues: previousStatus.ToString(), newValues: slot.Status.ToString(), userId: null, cancellationToken: cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return SlotTransferDto.FromEntity(transfer);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new ConflictException("CONCURRENCY_CONFLICT", "Otro usuario modificó la misma plaza simultáneamente. La operación fue revertida; inténtelo de nuevo.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<SlotTransferDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<SlotTransfer> items = await _context.SlotTransfers.AsNoTracking()
            .Include(t => t.RaceSlot).OrderByDescending(t => t.TransferDate).ToListAsync(cancellationToken);
        return items.Select(SlotTransferDto.FromEntity).ToList();
    }
}
