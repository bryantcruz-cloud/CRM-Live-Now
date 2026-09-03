using LiveNow.CRM.Core.Common;

namespace LiveNow.CRM.Core.Entities;

public class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ICollection<RaceSlot> Slots { get; set; } = new List<RaceSlot>();
    public ICollection<Hotel> Hotels { get; set; } = new List<Hotel>();
}
