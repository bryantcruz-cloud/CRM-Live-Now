using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public CustomerStatusEnum Status { get; set; } = CustomerStatusEnum.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string DisplayName => $"{FirstName} {LastName}".Trim();

    public static CustomerDto FromEntity(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            Country = entity.Country,
            City = entity.City,
            DateOfBirth = entity.DateOfBirth,
            Notes = entity.Notes,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive
        };
    }
}

public class CreateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCustomerDto
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
}
