using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class QuoteService : IQuoteService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public QuoteService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<PagedResult<QuoteDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        int safePage = Math.Max(page, 1);
        int safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        IQueryable<Quote> query = _context.Quotes.AsNoTracking();

        int totalCount = await query.CountAsync(cancellationToken);

        List<Quote> items = await query
            .Include(q => q.Items)
            .OrderByDescending(q => q.CreatedAt)
            .ThenBy(q => q.QuoteNumber)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<QuoteDto>.Create(
            items.Select(q => QuoteDto.FromEntity(q, q.Items.ToList())).ToList(),
            safePage,
            safePageSize,
            totalCount);
    }

    public async Task<QuoteDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Quote? quote = await _context.Quotes
            .AsNoTracking()
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken)
            ?? throw new NotFoundException("QUOTE_NOT_FOUND", "La cotización no existe.");

        return QuoteDto.FromEntity(quote, quote.Items.ToList());
    }

    public async Task<QuoteDto> CreateAsync(CreateQuoteDto dto, CancellationToken cancellationToken = default)
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
            throw new ValidationException("QUOTE_ITEMS_REQUIRED", "La cotización debe tener al menos un concepto.");
        }

        if (dto.Discount < 0m || dto.Taxes < 0m)
        {
            throw new ValidationException("QUOTE_TOTALS_INVALID", "El descuento y los impuestos no pueden ser negativos.");
        }

        Quote quote = new()
        {
            QuoteNumber = GenerateQuoteNumber(),
            CustomerId = dto.CustomerId,
            RaceEditionId = dto.RaceEditionId,
            Status = QuoteStatusEnum.Draft,
            Currency = dto.Currency,
            ValidUntil = dto.ValidUntil,
            Discount = dto.Discount,
            Taxes = dto.Taxes,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        foreach (CreateQuoteItemDto itemDto in dto.Items)
        {
            quote.Items.Add(CreateItem(itemDto));
        }

        RecalculateTotals(quote);

        _context.Quotes.Add(quote);
        await _auditService.RecordAsync("Quote", quote.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteDto.FromEntity(quote, quote.Items.ToList());
    }

    public async Task<QuoteDto> UpdateAsync(Guid id, UpdateQuoteDto dto, CancellationToken cancellationToken = default)
    {
        Quote quote = await GetTrackedAsync(id, cancellationToken);

        if (quote.Status is not (QuoteStatusEnum.Draft or QuoteStatusEnum.Sent))
        {
            throw new ConflictException("QUOTE_NOT_EDITABLE", $"Una cotización en estado {quote.Status} no puede modificarse.");
        }

        if (dto.Items.Count == 0)
        {
            throw new ValidationException("QUOTE_ITEMS_REQUIRED", "La cotización debe tener al menos un concepto.");
        }

        if (dto.Discount < 0m || dto.Taxes < 0m)
        {
            throw new ValidationException("QUOTE_TOTALS_INVALID", "El descuento y los impuestos no pueden ser negativos.");
        }

        _context.QuoteItems.RemoveRange(quote.Items);
        quote.Items.Clear();

        quote.Currency = dto.Currency;
        quote.ValidUntil = dto.ValidUntil;
        quote.Discount = dto.Discount;
        quote.Taxes = dto.Taxes;
        quote.Notes = dto.Notes;
        quote.UpdatedAt = DateTime.UtcNow;

        foreach (CreateQuoteItemDto itemDto in dto.Items)
        {
            quote.Items.Add(CreateItem(itemDto));
        }

        RecalculateTotals(quote);

        await _auditService.RecordAsync("Quote", quote.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QuoteDto.FromEntity(quote, quote.Items.ToList());
    }

    public async Task<QuoteDto> SendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Quote quote = await GetTrackedAsync(id, cancellationToken);

        if (quote.Status != QuoteStatusEnum.Draft)
        {
            throw new ConflictException("QUOTE_STATUS_TRANSITION_INVALID", $"Solo una cotización en borrador puede enviarse (estado actual: {quote.Status}).");
        }

        quote.Status = QuoteStatusEnum.Sent;
        quote.UpdatedAt = DateTime.UtcNow;

        await RecordTransitionAsync(quote.Id, QuoteStatusEnum.Draft, QuoteStatusEnum.Sent, cancellationToken);
        return QuoteDto.FromEntity(quote, quote.Items.ToList());
    }

    public async Task<QuoteDto> AcceptAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Quote quote = await GetTrackedAsync(id, cancellationToken);

        if (quote.Status != QuoteStatusEnum.Sent)
        {
            throw new ConflictException("QUOTE_STATUS_TRANSITION_INVALID", $"Solo una cotización enviada puede aceptarse (estado actual: {quote.Status}).");
        }

        quote.Status = QuoteStatusEnum.Accepted;
        quote.UpdatedAt = DateTime.UtcNow;

        await RecordTransitionAsync(quote.Id, QuoteStatusEnum.Sent, QuoteStatusEnum.Accepted, cancellationToken);
        return QuoteDto.FromEntity(quote, quote.Items.ToList());
    }

    public async Task<QuoteDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Quote quote = await GetTrackedAsync(id, cancellationToken);

        if (quote.Status is not (QuoteStatusEnum.Draft or QuoteStatusEnum.Sent))
        {
            throw new ConflictException("QUOTE_STATUS_TRANSITION_INVALID", $"Una cotización en estado {quote.Status} no puede cancelarse.");
        }

        QuoteStatusEnum previous = quote.Status;
        quote.Status = QuoteStatusEnum.Rejected;
        quote.UpdatedAt = DateTime.UtcNow;

        await RecordTransitionAsync(quote.Id, previous, QuoteStatusEnum.Rejected, cancellationToken);
        return QuoteDto.FromEntity(quote, quote.Items.ToList());
    }

    public async Task<QuoteDto> ConvertToSaleAsync(Guid id, ConvertQuoteToSaleDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            Quote quote = await GetTrackedAsync(id, cancellationToken);

            if (quote.Status != QuoteStatusEnum.Accepted)
            {
                throw new ConflictException("QUOTE_STATUS_INVALID", $"Solo una cotización aceptada puede convertirse en venta. Estado actual: {quote.Status}.");
            }

            bool alreadyConverted = await _context.Sales.AnyAsync(s => s.QuoteId == quote.Id, cancellationToken);
            if (alreadyConverted)
            {
                throw new ConflictException("QUOTE_ALREADY_CONVERTED", "Esta cotización ya fue convertida a venta.");
            }

            Sale sale = new()
            {
                SaleNumber = GenerateSaleNumber(),
                CustomerId = quote.CustomerId,
                RaceEditionId = quote.RaceEditionId,
                QuoteId = quote.Id,
                Currency = quote.Currency,
                SaleDate = DateTime.UtcNow,
                Subtotal = quote.Subtotal,
                Discount = quote.Discount,
                Taxes = quote.Taxes,
                TotalSalePrice = quote.Total,
                Status = SaleStatusEnum.Pending,
                Notes = quote.Notes
            };

            _context.Sales.Add(sale);

            foreach (QuoteItem quoteItem in quote.Items)
            {
                SaleItem saleItem = new()
                {
                    SaleId = sale.Id,
                    Description = quoteItem.Description,
                    ItemType = (SaleItemTypeEnum)(int)quoteItem.ItemType,
                    Quantity = quoteItem.Quantity,
                    UnitCost = quoteItem.UnitCost,
                    UnitPrice = quoteItem.UnitPrice,
                    TotalCost = quoteItem.TotalCost,
                    TotalPrice = quoteItem.TotalPrice
                };
                _context.SaleItems.Add(saleItem);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var entryItems = sale.Items
                .Where(item => item.ItemType == SaleItemTypeEnum.Entry)
                .ToList();

            if (dto.SlotAssignments.Count > entryItems.Sum(item => item.Quantity))
            {
                throw new ValidationException("RACE_SLOT_ASSIGNMENTS_TOO_MANY", "No se pueden asignar más plazas que entradas incluidas en la cotización.");
            }

            int assignmentIndex = 0;
            foreach (var assignment in dto.SlotAssignments)
            {
                RaceSlot? slot = await _context.RaceSlots
                    .FirstOrDefaultAsync(s => s.Id == assignment.SlotId, cancellationToken)
                    ?? throw new NotFoundException("RACE_SLOT_NOT_FOUND", $"La plaza {assignment.SlotId} no existe.");

                if (slot.RaceEditionId != sale.RaceEditionId)
                {
                    throw new ConflictException("RACE_SLOT_EDITION_MISMATCH", $"La plaza {slot.InternalCode} pertenece a otra edición.");
                }

                if (!RaceSlotService.CanBeSold(slot.Status))
                {
                    throw new ConflictException("RACE_SLOT_NOT_AVAILABLE", $"La plaza {slot.InternalCode} ya no está disponible (estado: {slot.Status}).");
                }

                SaleItem saleItem = entryItems[assignmentIndex++];
                saleItem.RaceSlotId = slot.Id;
                saleItem.UnitCost = slot.AcquisitionCost;
                saleItem.TotalCost = saleItem.UnitCost * saleItem.Quantity;

                slot.AssignedCustomerId = sale.CustomerId;
                slot.Status = SlotStatusEnum.Sold;
                slot.SaleDate = DateTime.UtcNow;
                slot.Version++;
            }

            sale.TotalCost = sale.Items.Sum(item => item.TotalCost);
            sale.TotalPaymentFees = 0;
            sale.GrossProfit = sale.TotalSalePrice - sale.TotalCost;
            sale.ProfitMargin = sale.TotalSalePrice > 0 ? sale.GrossProfit / sale.TotalSalePrice : 0;

            quote.Status = QuoteStatusEnum.ConvertedToSale;
            quote.UpdatedAt = DateTime.UtcNow;
            sale.Status = SaleStatusEnum.Confirmed;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return QuoteDto.FromEntity(quote, quote.Items.ToList());
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new ConflictException("CONCURRENCY_CONFLICT", "Conflicto de concurrencia: otra operación modificó los mismos datos. Intente nuevamente.");
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Quote> GetTrackedAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Quotes
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken)
            ?? throw new NotFoundException("QUOTE_NOT_FOUND", "La cotización no existe.");
    }

    private async Task RecordTransitionAsync(Guid quoteId, QuoteStatusEnum from, QuoteStatusEnum to, CancellationToken cancellationToken)
    {
        await _auditService.RecordAsync("Quote", quoteId.ToString(), AuditActionEnum.StatusChange, oldValues: from.ToString(), newValues: to.ToString(), userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static QuoteItem CreateItem(CreateQuoteItemDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ValidationException("QUOTE_ITEM_DESCRIPTION_REQUIRED", "La descripción del concepto es obligatoria.");
        }

        if (dto.Quantity <= 0)
        {
            throw new ValidationException("QUOTE_ITEM_QUANTITY_INVALID", "La cantidad debe ser mayor que cero.");
        }

        if (dto.UnitCost < 0m || dto.UnitPrice < 0m)
        {
            throw new ValidationException("QUOTE_ITEM_PRICE_INVALID", "El costo y el precio unitario no pueden ser negativos.");
        }

        return new QuoteItem
        {
            Description = dto.Description.Trim(),
            ItemType = dto.ItemType,
            Quantity = dto.Quantity,
            UnitCost = dto.UnitCost,
            UnitPrice = dto.UnitPrice,
            TotalCost = dto.UnitCost * dto.Quantity,
            TotalPrice = dto.UnitPrice * dto.Quantity,
            Notes = dto.Notes
        };
    }

    /// <summary>
    /// Subtotal = sum of item prices. Total = Subtotal - Discount + Taxes.
    /// Costs are stored per item and never mixed with prices.
    /// </summary>
    private static void RecalculateTotals(Quote quote)
    {
        quote.Subtotal = quote.Items.Sum(i => i.TotalPrice);
        quote.Total = quote.Subtotal - quote.Discount + quote.Taxes;
    }

    private static string GenerateQuoteNumber()
    {
        string suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"QT-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
    }

    private static string GenerateSaleNumber()
    {
        string suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"SL-{DateTime.UtcNow:yyyyMMdd}-{suffix}";
    }
}
