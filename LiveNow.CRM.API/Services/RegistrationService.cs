using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class RegistrationService : IRegistrationService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RegistrationService(LiveNowDbContext context, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<PagedResult<RegistrationDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        int safePage = Math.Max(page, 1);
        int safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        IQueryable<RunnerRegistration> query = _context.RunnerRegistrations.AsNoTracking();

        int totalCount = await query.CountAsync(cancellationToken);

        List<RunnerRegistration> items = await query
            .Include(r => r.Customer)
            .Include(r => r.RaceEdition)
                .ThenInclude(e => e.Race)
            .Include(r => r.RaceSlot)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<RegistrationDto>.Create(
            items.Select(RegistrationDto.FromEntity).ToList(),
            safePage,
            safePageSize,
            totalCount);
    }

    public async Task<RegistrationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RunnerRegistration? registration = await _context.RunnerRegistrations
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.RaceEdition)
                .ThenInclude(e => e.Race)
            .Include(r => r.RaceSlot)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("REGISTRATION_NOT_FOUND", "El registro del corredor no existe.");

        return RegistrationDto.FromEntity(registration);
    }

    public async Task<RegistrationDto> CreateAsync(CreateRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId, cancellationToken))
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        if (!await _context.RaceEditions.AnyAsync(e => e.Id == dto.RaceEditionId, cancellationToken))
        {
            throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición de la carrera no existe.");
        }

        RaceSlot? slot = await _context.RaceSlots.FirstOrDefaultAsync(s => s.Id == dto.RaceSlotId, cancellationToken)
            ?? throw new NotFoundException("RACE_SLOT_NOT_FOUND", "La plaza no existe.");

        if (slot.RaceEditionId != dto.RaceEditionId)
        {
            throw new ConflictException("RACE_SLOT_EDITION_MISMATCH", "La plaza pertenece a otra edición de la carrera.");
        }

        if (slot.AssignedCustomerId is not null && slot.AssignedCustomerId != dto.CustomerId)
        {
            throw new ConflictException("RACE_SLOT_ASSIGNED_TO_OTHER", "La plaza está asignada a otro cliente.");
        }

        RunnerRegistration registration = new()
        {
            CustomerId = dto.CustomerId,
            RaceEditionId = dto.RaceEditionId,
            RaceSlotId = dto.RaceSlotId,
            RegistrationStatus = dto.RegistrationStatus,
            RegistrationDate = dto.RegistrationStatus == RegistrationStatusEnum.Completed ? DateTime.UtcNow : null,
            ConfirmationNumber = dto.ConfirmationNumber,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.RunnerRegistrations.Add(registration);
        await _auditService.RecordAsync("RunnerRegistration", registration.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RegistrationDto.FromEntity(registration);
    }

    public async Task<RegistrationDto> UpdateAsync(Guid id, UpdateRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        RunnerRegistration? registration = await _context.RunnerRegistrations
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("REGISTRATION_NOT_FOUND", "El registro del corredor no existe.");

        RegistrationStatusEnum previous = registration.RegistrationStatus;

        if (previous == dto.RegistrationStatus &&
            registration.ConfirmationNumber == dto.ConfirmationNumber &&
            registration.Notes == dto.Notes &&
            registration.RegistrationDate == dto.RegistrationDate)
        {
            return RegistrationDto.FromEntity(registration);
        }

        if (previous is RegistrationStatusEnum.Cancelled or RegistrationStatusEnum.Completed &&
            dto.RegistrationStatus != previous)
        {
            throw new ConflictException("REGISTRATION_STATUS_TRANSITION_INVALID", $"Un registro {previous} no puede pasar a {dto.RegistrationStatus}.");
        }

        registration.RegistrationStatus = dto.RegistrationStatus;
        registration.ConfirmationNumber = dto.ConfirmationNumber;
        registration.Notes = dto.Notes;
        registration.UpdatedAt = DateTime.UtcNow;

        if (dto.RegistrationStatus == RegistrationStatusEnum.Completed && registration.RegistrationDate is null)
        {
            registration.RegistrationDate = dto.RegistrationDate ?? DateTime.UtcNow;
        }
        else if (dto.RegistrationStatus != RegistrationStatusEnum.Completed)
        {
            registration.RegistrationDate = dto.RegistrationDate;
        }

        await _auditService.RecordAsync(
            "RunnerRegistration",
            registration.Id.ToString(),
            AuditActionEnum.StatusChange,
            oldValues: previous.ToString(),
            newValues: dto.RegistrationStatus.ToString(),
            userId: null,
            cancellationToken: cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RegistrationDto.FromEntity(registration);
    }
}
