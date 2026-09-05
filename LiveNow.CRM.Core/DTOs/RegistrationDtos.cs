using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class RegistrationDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid RaceEditionId { get; set; }
    public string RaceEditionName { get; set; } = string.Empty;
    public Guid RaceSlotId { get; set; }
    public string RaceSlotCode { get; set; } = string.Empty;
    public RegistrationStatusEnum RegistrationStatus { get; set; } = RegistrationStatusEnum.Pending;
    public DateTime? RegistrationDate { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }

    public static RegistrationDto FromEntity(RunnerRegistration entity)
    {
        return new RegistrationDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            CustomerName = entity.Customer is null ? string.Empty : $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim(),
            RaceEditionId = entity.RaceEditionId,
            RaceEditionName = entity.RaceEdition is null ? string.Empty : entity.RaceEdition.Race is null ? entity.RaceEdition.Year.ToString() : $"{entity.RaceEdition.Race.Name} {entity.RaceEdition.Year}",
            RaceSlotId = entity.RaceSlotId,
            RaceSlotCode = entity.RaceSlot?.InternalCode ?? string.Empty,
            RegistrationStatus = entity.RegistrationStatus,
            RegistrationDate = entity.RegistrationDate,
            ConfirmationNumber = entity.ConfirmationNumber,
            Notes = entity.Notes
        };
    }
}

public class CreateRegistrationDto
{
    public Guid CustomerId { get; set; }
    public Guid RaceEditionId { get; set; }
    public Guid RaceSlotId { get; set; }
    public RegistrationStatusEnum RegistrationStatus { get; set; } = RegistrationStatusEnum.Pending;
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }
}

public class UpdateRegistrationDto
{
    public RegistrationStatusEnum RegistrationStatus { get; set; } = RegistrationStatusEnum.Pending;
    public DateTime? RegistrationDate { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }
}
