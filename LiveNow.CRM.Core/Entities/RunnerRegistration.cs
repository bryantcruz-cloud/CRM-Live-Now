using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class RunnerRegistration : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid RaceEditionId { get; set; }
    public Guid RaceSlotId { get; set; }
    public RegistrationStatusEnum RegistrationStatus { get; set; } = RegistrationStatusEnum.Pending;
    public DateTime? RegistrationDate { get; set; }
    public string? ConfirmationNumber { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = null!;
    public RaceEdition RaceEdition { get; set; } = null!;
    public RaceSlot RaceSlot { get; set; } = null!;
}
