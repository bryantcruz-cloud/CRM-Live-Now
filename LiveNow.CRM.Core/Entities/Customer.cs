using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.Entities;

public class Customer : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public CustomerStatusEnum Status { get; set; } = CustomerStatusEnum.Active;

    // Navigation properties
    public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<HotelReservation> HotelReservations { get; set; } = new List<HotelReservation>();
    public ICollection<RunnerRegistration> RunnerRegistrations { get; set; } = new List<RunnerRegistration>();
    public ICollection<CustomerChecklist> Checklists { get; set; } = new List<CustomerChecklist>();
    public ICollection<Cancellation> Cancellations { get; set; } = new List<Cancellation>();
}
