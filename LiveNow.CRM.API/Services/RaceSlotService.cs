using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class RaceSlotService : IRaceSlotService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RaceSlotService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<PagedResult<RaceSlotDto>> GetByEditionAsync(
        Guid raceEditionId,
        int page,
        int pageSize,
        SlotStatusEnum? status,
        CancellationToken cancellationToken = default)
    {
        int safePage = Math.Max(page, 1);
        int safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        IQueryable<RaceSlot> query = _context.RaceSlots.AsNoTracking().Where(s => s.RaceEditionId == raceEditionId);

        if (status is not null)
        {
            query = query.Where(s => s.Status == status);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<RaceSlot> items = await query
            .OrderBy(s => s.InternalCode)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<RaceSlotDto>.Create(
            items.Select(RaceSlotDto.FromEntity).ToList(),
            safePage,
            safePageSize,
            totalCount);
    }

    public async Task<RaceSlotDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RaceSlot? slot = await _context.RaceSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_SLOT_NOT_FOUND", "La plaza no existe.");

        return RaceSlotDto.FromEntity(slot);
    }

    public async Task<RaceSlotDto> CreateAsync(CreateRaceSlotDto dto, CancellationToken cancellationToken = default)
    {
        bool editionExists = await _context.RaceEditions.AnyAsync(e => e.Id == dto.RaceEditionId, cancellationToken);
        if (!editionExists)
        {
            throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición no existe.");
        }

        if (string.IsNullOrWhiteSpace(dto.InternalCode))
        {
            throw new ValidationException("RACE_SLOT_CODE_REQUIRED", "El código interno es obligatorio.");
        }

        if (dto.AcquisitionCost < 0m)
        {
            throw new ValidationException("RACE_SLOT_COST_INVALID", "El costo de adquisición no puede ser negativo.");
        }

        if (await _context.RaceSlots.AnyAsync(s => s.InternalCode == dto.InternalCode.Trim(), cancellationToken))
        {
            throw new ConflictException("RACE_SLOT_CODE_EXISTS", "Ya existe una plaza con ese código interno.");
        }

        RaceSlot slot = new()
        {
            RaceEditionId = dto.RaceEditionId,
            InternalCode = dto.InternalCode.Trim(),
            Status = dto.Status,
            AcquisitionCost = dto.AcquisitionCost,
            AcquisitionCurrency = dto.AcquisitionCurrency,
            SupplierId = dto.SupplierId,
            AssignedCustomerId = dto.AssignedCustomerId,
            ReservationDate = dto.ReservationDate,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            Version = 0
        };

        _context.RaceSlots.Add(slot);
        await _auditService.RecordAsync("RaceSlot", slot.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RaceSlotDto.FromEntity(slot);
    }

    public async Task<RaceSlotDto> UpdateAsync(Guid id, UpdateRaceSlotDto dto, CancellationToken cancellationToken = default)
    {
        RaceSlot? slot = await _context.RaceSlots
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_SLOT_NOT_FOUND", "La plaza no existe.");

        ValidateStateTransition(slot.Status, dto.Status);

        slot.Status = dto.Status;
        slot.AcquisitionCost = dto.AcquisitionCost;
        slot.AcquisitionCurrency = dto.AcquisitionCurrency;
        slot.SupplierId = dto.SupplierId;
        slot.AssignedCustomerId = dto.AssignedCustomerId;
        slot.ReservationDate = dto.ReservationDate;
        slot.Notes = dto.Notes;
        slot.UpdatedAt = DateTime.UtcNow;
        slot.Version++;

        await _auditService.RecordAsync("RaceSlot", slot.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RaceSlotDto.FromEntity(slot);
    }

    public async Task<InventorySummaryDto> GetInventorySummaryAsync(Guid raceEditionId, CancellationToken cancellationToken = default)
    {
        bool editionExists = await _context.RaceEditions.AnyAsync(e => e.Id == raceEditionId, cancellationToken);
        if (!editionExists)
        {
            throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición no existe.");
        }

        List<RaceSlot> slots = await _context.RaceSlots
            .AsNoTracking()
            .Where(s => s.RaceEditionId == raceEditionId)
            .ToListAsync(cancellationToken);

        int CountOf(SlotStatusEnum status) => slots.Count(s => s.Status == status);

        return new InventorySummaryDto
        {
            RaceEditionId = raceEditionId,
            Total = slots.Count,
            Available = CountOf(SlotStatusEnum.Available),
            AvailableCount = CountOf(SlotStatusEnum.Available),
            Reserved = CountOf(SlotStatusEnum.Reserved),
            ReservedCount = CountOf(SlotStatusEnum.Reserved),
            Sold = CountOf(SlotStatusEnum.Sold),
            SoldCount = CountOf(SlotStatusEnum.Sold),
            Registered = CountOf(SlotStatusEnum.Registered),
            RegisteredCount = CountOf(SlotStatusEnum.Registered),
            Cancelled = CountOf(SlotStatusEnum.Cancelled),
            Injured = CountOf(SlotStatusEnum.Injured),
            Transferable = CountOf(SlotStatusEnum.Transferable),
            Transferred = CountOf(SlotStatusEnum.Transferred),
            Lost = CountOf(SlotStatusEnum.Lost)
        };
    }

    public static bool CanBeSold(SlotStatusEnum status)
    {
        return status == SlotStatusEnum.Available ||
               status == SlotStatusEnum.Reserved ||
               status == SlotStatusEnum.Transferable;
    }

    private static void ValidateStateTransition(SlotStatusEnum current, SlotStatusEnum next)
    {
        if (current == next)
        {
            return;
        }

        bool allowed = current switch
        {
            SlotStatusEnum.Available => next is SlotStatusEnum.Reserved or SlotStatusEnum.Sold or SlotStatusEnum.Cancelled,
            SlotStatusEnum.Reserved => next is SlotStatusEnum.Available or SlotStatusEnum.Sold or SlotStatusEnum.Cancelled,
            SlotStatusEnum.Sold => next is SlotStatusEnum.Registered or SlotStatusEnum.Cancelled or SlotStatusEnum.Injured or SlotStatusEnum.Transferable,
            SlotStatusEnum.Registered => next is SlotStatusEnum.Cancelled or SlotStatusEnum.Injured or SlotStatusEnum.Transferred,
            SlotStatusEnum.Cancelled => next is SlotStatusEnum.Available or SlotStatusEnum.Transferable,
            SlotStatusEnum.Injured => next is SlotStatusEnum.Cancelled,
            SlotStatusEnum.Transferable => next is SlotStatusEnum.Sold or SlotStatusEnum.Transferred,
            SlotStatusEnum.Transferred => false,
            SlotStatusEnum.Lost => false,
            _ => false
        };

        if (!allowed)
        {
            throw new ConflictException(
                "RACE_SLOT_STATUS_TRANSITION_INVALID",
                $"La transición de estado de {current} a {next} no está permitida.");
        }
    }
}