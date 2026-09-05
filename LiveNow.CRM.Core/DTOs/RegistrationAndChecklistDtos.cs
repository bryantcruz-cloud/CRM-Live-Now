using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;

namespace LiveNow.CRM.Core.DTOs;

public class ChecklistItemDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid SaleId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string SaleNumber { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    public static ChecklistItemDto FromEntity(CustomerChecklist entity)
    {
        return new ChecklistItemDto
        {
            Id = entity.Id,
            CustomerId = entity.CustomerId,
            SaleId = entity.SaleId,
            CustomerName = entity.Customer is null ? string.Empty : $"{entity.Customer.FirstName} {entity.Customer.LastName}".Trim(),
            SaleNumber = entity.Sale?.SaleNumber ?? string.Empty,
            ItemType = entity.ItemType,
            Description = entity.Description,
            IsCompleted = entity.IsCompleted,
            CompletedAt = entity.CompletedAt,
            Notes = entity.Notes
        };
    }
}

public class CreateChecklistItemDto
{
    public Guid SaleId { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateChecklistItemDto
{
    public string ItemType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}
