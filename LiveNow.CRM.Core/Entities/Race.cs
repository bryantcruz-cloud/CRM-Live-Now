using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class Race : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public RaceTypeEnum RaceType { get; set; } = RaceTypeEnum.Marathon;
    public RaceStatusEnum Status { get; set; } = RaceStatusEnum.Active;

    // Navigation properties
    public ICollection<RaceEdition> Editions { get; set; } = new List<RaceEdition>();
}
