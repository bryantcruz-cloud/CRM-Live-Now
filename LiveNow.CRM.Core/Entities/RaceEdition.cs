using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class RaceEdition : BaseEntity
{
    public Guid RaceId { get; set; }
    public int Year { get; set; }
    public DateTime? RaceDate { get; set; }
    public CurrencyEnum Currency { get; set; } = CurrencyEnum.USD;
    public RaceEditionStatusEnum Status { get; set; } = RaceEditionStatusEnum.Planning;
    public string? Notes { get; set; }

    // Navigation properties
    public Race Race { get; set; } = null!;
    public ICollection<RaceSlot> Slots { get; set; } = new List<RaceSlot>();
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<RunnerRegistration> RunnerRegistrations { get; set; } = new List<RunnerRegistration>();
}
