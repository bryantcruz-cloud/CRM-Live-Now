using LiveNow.CRM.Core.Common;

namespace LiveNow.CRM.Core.Entities;

public class Hotel : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Supplier? Supplier { get; set; }
    public ICollection<HotelReservation> Reservations { get; set; } = new List<HotelReservation>();
}
