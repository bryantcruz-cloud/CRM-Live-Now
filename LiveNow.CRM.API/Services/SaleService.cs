using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class SaleService : ISaleService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IFinancialCalculator _financialCalculator;

    public SaleService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        IFinancialCalculator financialCalculator)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _financialCalculator = financialCalculator;
    }

    public async Task<PagedResult<SaleDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        int safePage = Math.Max(page, 1);
        int safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        IQueryable<Sale> query = _context.Sales.AsNoTracking();

        int totalCount = await query.CountAsync(cancellationToken);

        List<Sale> items = await query
            .Include(s => s.Customer)
            .Include(s => s.RaceEdition)
                .ThenInclude(e => e.Race)
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .ThenBy(s => s.SaleNumber)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<SaleDto>.Create(
            items.Select(s => SaleDto.FromEntity(s, s.Items.ToList())).ToList(),
            safePage,
            safePageSize,
            totalCount);
    }

    public async Task<SaleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Sale? sale = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.RaceEdition)
                .ThenInclude(e => e.Race)
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");

        return SaleDto.FromEntity(sale, sale.Items.ToList());
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto dto, CancellationToken cancellationToken = default)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId, cancellationToken))
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        if (!await _context.RaceEditions.AnyAsync(e => e.Id == dto.RaceEditionId, cancellationToken))
        {
            throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición de la carrera no existe.");
        }

        if (dto.Items.Count == 0)
        {
            throw new ValidationException("SALE_ITEMS_REQUIRED", "La venta debe tener al menos un concepto.");
        }

        if (dto.Discount < 0m || dto.Taxes < 0m)
        {
            throw new ValidationException("SALE_TOTALS_INVALID", "El descuento y los impuestos no pueden ser negativos.");
        }

        Quote? quote = null;
        if (dto.QuoteId is not null)
        {
            quote = await _context.Quotes.FirstOrDefaultAsync(q => q.Id == dto.QuoteId, cancellationToken)
                ?? throw new NotFoundException("QUOTE_NOT_FOUND", "La cotización indicada no existe.");

            if (quote.CustomerId != dto.CustomerId)
            {
                throw new ConflictException("QUOTE_CUSTOMER_MISMATCH", "La cotización pertenece a otro cliente.");
            }
        }

        Sale sale = new()
        {
            SaleNumber = GenerateSaleNumber(),
            CustomerId = dto.CustomerId,
            RaceEditionId = dto.RaceEditionId,
            QuoteId = dto.QuoteId,
            Currency = dto.Currency,
            SaleDate = DateTime.UtcNow,
            Discount = dto.Discount,
            Taxes = dto.Taxes,
            Status = SaleStatusEnum.Pending,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (CreateSaleItemDto itemDto in dto.Items)
        {
            sale.Items.Add(CreateItem(itemDto));
        }

            RecalculateTotals(sale);
        SaleFinancials.Apply(sale, _financialCalculator);

        _context.Sales.Add(sale);
        await _auditService.RecordAsync("Sale", sale.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SaleDto.FromEntity(sale, sale.Items.ToList());
    }

    public async Task<SaleDto> UpdateAsync(Guid id, UpdateSaleDto dto, CancellationToken cancellationToken = default)
    {
        Sale sale = await GetTrackedWithFinancialsAsync(id, cancellationToken);

        if (sale.Status is not SaleStatusEnum.Pending)
        {
            throw new ConflictException("SALE_NOT_EDITABLE", $"Una venta en estado {sale.Status} no puede modificarse.");
        }

        if (dto.Discount < 0m || dto.Taxes < 0m)
        {
            throw new ValidationException("SALE_TOTALS_INVALID", "El descuento y los impuestos no pueden ser negativos.");
        }

        sale.Discount = dto.Discount;
        sale.Taxes = dto.Taxes;
        sale.Notes = dto.Notes;
        sale.UpdatedAt = DateTime.UtcNow;

        RecalculateTotals(sale);
        SaleFinancials.Apply(sale, _financialCalculator);

        await _auditService.RecordAsync("Sale", sale.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SaleDto.FromEntity(sale, sale.Items.ToList());
    }

    /// <summary>
    /// Confirms a pending sale inside a database transaction:
    /// validates customer/edition/slots, assigns slots to the customer,
    /// updates slot states, recalculates all financial figures and audits everything.
    /// If any step fails, the whole operation is rolled back.
    /// Slot assignment is protected by the optimistic concurrency token (RaceSlot.Version):
    /// if two machines confirm the same slot simultaneously, the second SaveChanges
    /// throws DbUpdateConcurrencyException and the transaction is rolled back (HTTP 409).
    /// </summary>
    public async Task<SaleDto> ConfirmAsync(Guid id, ConfirmSaleDto dto, CancellationToken cancellationToken = default)
    {
        Sale sale = await GetTrackedWithFinancialsAsync(id, cancellationToken, includePayments: true);

        if (sale.Status is not SaleStatusEnum.Pending)
        {
            throw new ConflictException("SALE_NOT_CONFIRMABLE", $"Solo una venta pendiente puede confirmarse (estado actual: {sale.Status}).");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await ValidateReferencesAsync(sale, cancellationToken);
            await AssignSlotsAsync(sale, dto, cancellationToken);

            RecalculateTotals(sale);

            decimal paidAmount = SaleFinancials.SumValidPayments(sale.Payments);
            SaleFinancials.Apply(sale, _financialCalculator);

            Quote? quote = sale.QuoteId is null
                ? null
                : await _context.Quotes.FirstOrDefaultAsync(q => q.Id == sale.QuoteId, cancellationToken);

            sale.Status = SaleStatusEnum.Confirmed;
            sale.UpdatedAt = DateTime.UtcNow;
            SaleFinancials.UpdateStatusFromPayments(sale, paidAmount);

            await _auditService.RecordAsync("Sale", sale.Id.ToString(), AuditActionEnum.StatusChange, oldValues: SaleStatusEnum.Pending.ToString(), newValues: sale.Status.ToString(), userId: null, cancellationToken: cancellationToken);

            if (quote is not null && quote.Status != QuoteStatusEnum.ConvertedToSale)
            {
                quote.Status = QuoteStatusEnum.ConvertedToSale;
                quote.UpdatedAt = DateTime.UtcNow;
                await _auditService.RecordAsync("Quote", quote.Id.ToString(), AuditActionEnum.StatusChange, oldValues: quote.Status.ToString(), newValues: QuoteStatusEnum.ConvertedToSale.ToString(), userId: null, cancellationToken: cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return SaleDto.FromEntity(sale, sale.Items.ToList());
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new ConflictException("CONCURRENCY_CONFLICT", "Otro usuario modificó el mismo registro simultáneamente. La operación fue revertida; inténtelo de nuevo.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task ValidateReferencesAsync(Sale sale, CancellationToken cancellationToken)
    {
        Customer? customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == sale.CustomerId, cancellationToken);
        if (customer is null)
        {
            throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");
        }

        if (customer.Status != CustomerStatusEnum.Active)
        {
            throw new ConflictException("CUSTOMER_INACTIVE", $"El cliente {customer.FirstName} {customer.LastName} no está activo.");
        }

        bool editionExists = await _context.RaceEditions.AnyAsync(e => e.Id == sale.RaceEditionId, cancellationToken);
        if (!editionExists)
        {
            throw new NotFoundException("RACE_EDITION_NOT_FOUND", "La edición de la carrera no existe.");
        }
    }

    private async Task AssignSlotsAsync(Sale sale, ConfirmSaleDto dto, CancellationToken cancellationToken)
    {
        if (dto.Slots.Count == 0)
        {
            return;
        }

        var entryItems = sale.Items
            .Where(i => i.ItemType == SaleItemTypeEnum.Entry && i.RaceSlotId is null)
            .OrderBy(i => i.CreatedAt)
            .ToList();

        if (dto.Slots.Count > entryItems.Count)
        {
            throw new ConflictException("SALE_SLOTS_MISMATCH", "Hay más plazas indicadas que conceptos de entrada disponibles en la venta.");
        }

        for (int index = 0; index < dto.Slots.Count; index++)
        {
            ConfirmSlotPriceDto slotDto = dto.Slots[index];
            SaleItem item = entryItems[index];

            RaceSlot? slot = await _context.RaceSlots.FirstOrDefaultAsync(s => s.Id == slotDto.SlotId, cancellationToken);
            if (slot is null)
            {
                throw new NotFoundException("RACE_SLOT_NOT_FOUND", $"La plaza {slotDto.SlotId} no existe.");
            }

            if (slot.RaceEditionId != sale.RaceEditionId)
            {
                throw new ConflictException("RACE_SLOT_EDITION_MISMATCH", "La plaza pertenece a otra edición de la carrera.");
            }

            if (!RaceSlotService.CanBeSold(slot.Status))
            {
                throw new ConflictException("RACE_SLOT_NOT_AVAILABLE", $"La plaza {slot.InternalCode} ya no está disponible (estado actual: {slot.Status}).");
            }

            SlotStatusEnum previousStatus = slot.Status;

            item.RaceSlotId = slot.Id;
            item.UnitCost = slot.AcquisitionCost;
            item.TotalCost = item.UnitCost * item.Quantity;

            if (slotDto.UnitPrice > 0m)
            {
                item.UnitPrice = slotDto.UnitPrice;
                item.TotalPrice = item.UnitPrice * item.Quantity;
            }

            slot.AssignedCustomerId = sale.CustomerId;
            slot.Status = SlotStatusEnum.Sold;
            slot.SaleDate = DateTime.UtcNow;
            slot.Version++; // optimistic concurrency token

            await _auditService.RecordAsync("RaceSlot", slot.Id.ToString(), AuditActionEnum.StatusChange, oldValues: previousStatus.ToString(), newValues: SlotStatusEnum.Sold.ToString(), userId: null, cancellationToken: cancellationToken);
        }
    }

    private async Task<Sale> GetTrackedWithFinancialsAsync(Guid id, CancellationToken cancellationToken, bool includePayments = false)
    {
        IQueryable<Sale> query = _context.Sales.Include(s => s.Items);

        if (includePayments)
        {
            query = query.Include(s => s.Payments).ThenInclude(p => p.Fees);
        }

        return await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");
    }

    private static SaleItem CreateItem(CreateSaleItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("SALE_ITEM_DESCRIPTION_REQUIRED", "La descripción del concepto es obligatoria.");
        }

        if (dto.Quantity <= 0)
        {
            throw new ValidationException("SALE_ITEM_QUANTITY_INVALID", "La cantidad debe ser mayor que cero.");
        }

        if (dto.UnitCost < 0m || dto.UnitPrice < 0m)
        {
            throw new ValidationException("SALE_ITEM_PRICE_INVALID", "El costo y el precio unitario no pueden ser negativos.");
        }

        return new SaleItem
        {
            Description = dto.Description.Trim(),
            ItemType = dto.ItemType,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            UnitPrice = dto.UnitPrice,
            TotalCost = dto.UnitCost * dto.Quantity,
            TotalPrice = dto.UnitPrice * dto.Quantity
        };
    }

    /// <summary>
    /// Subtotal = sum of item prices. TotalSalePrice = Subtotal - Discount + Taxes.
    /// Costs are recalculated from the items, never from stored aggregates.
    /// </summary>
    private static void RecalculateTotals(Sale sale)
    {
        sale.Subtotal = SaleFinancials.SumItemsPrice(sale.Items);
        sale.TotalSalePrice = sale.Subtotal - sale.Discount + sale.Taxes;
    }

    private static string GenerateSaleNumber()
    {
        string suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"SL-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
    }
}
