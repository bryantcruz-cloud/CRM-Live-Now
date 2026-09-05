using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class ChecklistService : IChecklistService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public ChecklistService(LiveNowDbContext context, IUnitOfWork unitOfWork, IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<IReadOnlyList<ChecklistItemDto>> GetByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        List<CustomerChecklist> items = await _context.CustomerChecklists
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId)
            .OrderBy(i => i.ItemType)
            .ThenBy(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.Select(ChecklistItemDto.FromEntity).ToList();
    }

    /// <summary>
    /// New checklist items can be added freely: ItemType and Description are free-form,
    /// so operations can define new items without code changes.
    /// </summary>
    public async Task<ChecklistItemDto> CreateAsync(Guid customerId, CreateChecklistItemDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == customerId, cancellationToken))
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        if (!await _context.Sales.AnyAsync(s => s.Id == dto.SaleId, cancellationToken))
        {
            throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("CHECKLIST_DESCRIPTION_REQUIRED", "La descripción del elemento es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(dto.ItemType))
        {
            throw new ValidationException("CHECKLIST_ITEM_TYPE_REQUIRED", "El tipo de elemento es obligatorio.");
        }

        CustomerChecklist item = new()
        {
            CustomerId = customerId,
            SaleId = dto.SaleId,
            ItemType = dto.ItemType.Trim(),
            Description = dto.Description.Trim(),
            IsCompleted = false,
            CompletedAt = null,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.CustomerChecklists.Add(item);
        await _auditService.RecordAsync("CustomerChecklist", item.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ChecklistItemDto.FromEntity(item);
    }

    public async Task<ChecklistItemDto> UpdateAsync(Guid id, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default)
    {
        CustomerChecklist? item = await _context.CustomerChecklists
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException("CHECKLIST_ITEM_NOT_FOUND", "El elemento del checklist no existe.");

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("CHECKLIST_DESCRIPTION_REQUIRED", "La descripción del elemento es obligatoria.");
        }

        bool wasCompleted = item.IsCompleted;

        item.ItemType = dto.ItemType.Trim();
        item.Description = dto.Description.Trim();
        item.Notes = dto.Notes;

        // CompletedAt is stamped automatically when the item is marked complete,
        // and cleared when it is unchecked.
        item.IsCompleted = dto.IsCompleted;
        item.CompletedAt = dto.IsCompleted
            ? (item.CompletedAt ?? DateTime.UtcNow)
            : null;

        item.UpdatedAt = DateTime.UtcNow;

        if (wasCompleted != item.IsCompleted)
        {
            await _auditService.RecordAsync(
                "CustomerChecklist",
                item.Id.ToString(),
                AuditActionEnum.StatusChange,
                oldValues: $"IsCompleted={wasCompleted}",
                newValues: $"IsCompleted={item.IsCompleted}",
                userId: null,
                cancellationToken: cancellationToken);
        }
        else
        {
            await _auditService.RecordAsync("CustomerChecklist", item.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ChecklistItemDto.FromEntity(item);
    }
}
