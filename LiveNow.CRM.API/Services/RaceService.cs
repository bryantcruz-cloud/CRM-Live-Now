using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class RaceService : IRaceService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public RaceService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<RaceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Race> races = await _context.Races
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        return races.Select(RaceDto.FromEntity).ToList();
    }

    public async Task<RaceDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Race? race = await _context.Races
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_NOT_FOUND", "La carrera no existe.");

        return RaceDto.FromEntity(race);
    }

    public async Task<RaceDto> CreateAsync(CreateRaceDto dto, CancellationToken cancellationToken = default)
    {
        ValidateName(dto.Name);

        if (await _context.Races.AnyAsync(r => r.Name.ToLower() == dto.Name.Trim().ToLower(), cancellationToken))
        {
            throw new ConflictException("RACE_NAME_EXISTS", "Ya existe una carrera con ese nombre.");
        }

        Race race = new()
        {
            Name = dto.Name.Trim(),
            City = dto.City.Trim(),
            Country = dto.Country.Trim(),
            RaceType = dto.RaceType,
            Status = RaceStatusEnum.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Races.Add(race);
        await _auditService.RecordAsync("Race", race.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RaceDto.FromEntity(race);
    }

    public async Task<RaceDto> UpdateAsync(Guid id, UpdateRaceDto dto, CancellationToken cancellationToken = default)
    {
        ValidateName(dto.Name);

        Race? race = await _context.Races
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_NOT_FOUND", "La carrera no existe.");

        race.Name = dto.Name.Trim();
        race.City = dto.City.Trim();
        race.Country = dto.Country.Trim();
        race.RaceType = dto.RaceType;
        race.Status = dto.Status;
        race.UpdatedAt = DateTime.UtcNow;

        await _auditService.RecordAsync("Race", race.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RaceDto.FromEntity(race);
    }

    public async Task<IReadOnlyList<RaceEditionDto>> GetEditionsAsync(Guid raceId, CancellationToken cancellationToken = default)
    {
        bool raceExists = await _context.Races.AnyAsync(r => r.Id == raceId, cancellationToken);
        if (!raceExists)
        {
            throw new NotFoundException("RACE_NOT_FOUND", "La carrera no existe.");
        }

        List<RaceEdition> editions = await _context.RaceEditions
            .AsNoTracking()
            .Where(e => e.RaceId == raceId)
            .OrderByDescending(e => e.Year)
            .ToListAsync(cancellationToken);

        return editions.Select(RaceEditionDto.FromEntity).ToList();
    }

    public async Task<RaceEditionDto> CreateEditionAsync(Guid raceId, CreateRaceEditionDto dto, CancellationToken cancellationToken = default)
    {
        bool raceExists = await _context.Races.AnyAsync(r => r.Id == raceId, cancellationToken);
        if (!raceExists)
        {
            throw new NotFoundException("RACE_NOT_FOUND", "La carrera no existe.");
        }

        if (dto.Year <= 0)
        {
            throw new ValidationException("RACE_EDITION_YEAR_INVALID", "El año de la edición es obligatorio.");
        }

        if (await _context.RaceEditions.AnyAsync(e => e.RaceId == raceId && e.Year == dto.Year, cancellationToken))
        {
            throw new ConflictException("RACE_EDITION_YEAR_EXISTS", "Ya existe una edición de esta carrera para ese año.");
        }

        RaceEdition edition = new()
        {
            RaceId = raceId,
            Year = dto.Year,
            RaceDate = dto.RaceDate,
            Currency = dto.Currency,
            Status = dto.Status,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.RaceEditions.Add(edition);
        await _auditService.RecordAsync("RaceEdition", edition.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return RaceEditionDto.FromEntity(edition);
    }

    public async Task<RaceEditionDto> GetEditionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RaceEdition? edition = await _context.RaceEditions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición no existe.");

        return RaceEditionDto.FromEntity(edition);
    }

    public async Task<RaceEditionDto> UpdateEditionAsync(Guid id, UpdateRaceEditionDto dto, CancellationToken cancellationToken = default)
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

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("RACE_NAME_REQUIRED", "El nombre de la carrera es obligatorio.");
        }
    }
}