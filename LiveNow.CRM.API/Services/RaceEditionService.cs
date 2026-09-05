using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class RaceEditionService : IRaceEditionService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RaceEditionService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<RaceEditionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RaceEdition? edition = await _context.RaceEditions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición no existe.");

        return RaceEditionDto.FromEntity(edition);
    }

    public async Task<RaceEditionDto> UpdateAsync(Guid id, UpdateRaceEditionDto dto, CancellationToken cancellationToken = default)
    {
        RaceEdition? edition = await _context.RaceEditions
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición no existe.");

        edition.RaceDate = dto.RaceDate;
        edition.Currency = dto.Currency;
        edition.Status = dto.Status;
        edition.Notes = dto.Notes;
        edition.UpdatedAt = DateTime.UtcNow;

        await _auditService.RecordAsync("RaceEdition", edition.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RaceEditionDto.FromEntity(edition);
    }
}