using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class RaceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public RaceTypeEnum RaceType { get; set; } = RaceTypeEnum.Marathon;
    public RaceStatusEnum Status { get; set; } = RaceStatusEnum.Active;
    public bool IsActive { get; set; } = true;

    public static RaceDto FromEntity(Race entity)
    {
        return new RaceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            City = entity.City,
            Country = entity.Country,
            RaceType = entity.RaceType,
            Status = entity.Status,
            IsActive = entity.IsActive
        };
    }
}

public class CreateRaceDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public RaceTypeEnum RaceType { get; set; } = RaceTypeEnum.Marathon;
}

public class UpdateRaceDto
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public RaceTypeEnum RaceType { get; set; } = RaceTypeEnum.Marathon;
    public RaceStatusEnum Status { get; set; } = RaceStatusEnum.Active;
}

public class RaceEditionDto
{
    public Guid Id { get; set; }
    public Guid RaceId { get; set; }
    public int Year { get; set; }
    public DateTime? RaceDate { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public RaceEditionStatusEnum Status { get; set; } = RaceEditionStatusEnum.Planning;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public static RaceEditionDto FromEntity(RaceEdition entity)
    {
        return new RaceEditionDto
        {
            Id = entity.Id,
            RaceId = entity.RaceId,
            Year = entity.Year,
            RaceDate = entity.RaceDate,
            Currency = entity.Currency,
            Status = entity.Status,
            Notes = entity.Notes,
            IsActive = entity.IsActive
        };
    }
}

public class CreateRaceEditionDto
{
    public int Year { get; set; }
    public DateTime? RaceDate { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public RaceEditionStatusEnum Status { get; set; } = RaceEditionStatusEnum.Planning;
    public string? Notes { get; set; }
}

public class UpdateRaceEditionDto
{
    public DateTime? RaceDate { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public RaceEditionStatusEnum Status { get; set; } = RaceEditionStatusEnum.Planning;
    public string? Notes { get; set; }
}