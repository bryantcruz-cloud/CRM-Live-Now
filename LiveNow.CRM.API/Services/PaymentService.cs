using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class PaymentService : IPaymentService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly IFinancialCalculator _financialCalculator;
    private readonly IPaymentFeeCalculator _feeCalculator;

    public PaymentService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        IFinancialCalculator financialCalculator,
        IPaymentFeeCalculator feeCalculator)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _financialCalculator = financialCalculator;
        _feeCalculator = feeCalculator;
    }

    public async Task<PagedResult<PaymentDto>> GetBySaleAsync(Guid saleId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (!await _context.Sales.AnyAsync(s => s.Id == saleId, cancellationToken))
        {
            throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");
        }

        int safePage = Math.Max(page, 1);
        int safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        IQueryable<Payment> query = _context.Payments.AsNoTracking().Where(p => p.SaleId == saleId);

        int totalCount = await query.CountAsync(cancellationToken);

        List<Payment> items = await query
            .Include(p => p.Fees)
            .OrderByDescending(p => p.PaymentDate)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PaymentDto>.Create(
            items.Select(p => PaymentDto.FromEntity(p, p.Fees.ToList())).ToList(),
            safePage,
            safePageSize,
            totalCount);
    }

    /// <summary>
    /// Registers a payment for a sale inside a transaction.
    /// Rules: amount must be positive; a payment that exceeds the outstanding
    /// balance is rejected (credits/customer balances may be added later).
    /// If a fee definition is provided, the PaymentFee is calculated centrally
    /// by <see cref="IPaymentFeeCalculator"/> (percentage, fixed, both or none).
    /// Sale financial figures are always recomputed from source data.
    /// </summary>
    public async Task<PaymentDto> RegisterPaymentAsync(Guid saleId, CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        Sale sale = await GetSaleWithFinancialsAsync(saleId, cancellationToken);

        if (sale.Status is SaleStatusEnum.Cancelled or SaleStatusEnum.Refunded)
        {
            throw new ConflictException("SALE_NOT_PAYABLE", $"No se pueden registrar pagos sobre una venta {sale.Status}.");
        }

        if (dto.Amount <= 0m)
        {
            throw new ValidationException("PAYMENT_AMOUNT_INVALID", "El monto del pago debe ser mayor que cero.");
        }

        if (dto.Amount != decimal.Round(dto.Amount, 2))
        {
            throw new ValidationException("PAYMENT_AMOUNT_PRECISION", "El monto del pago no puede tener más de 2 decimales.");
        }

        if (dto.PaymentDate > DateTime.UtcNow.AddDays(1))
        {
            throw new ValidationException("PAYMENT_DATE_INVALID", "La fecha del pago no puede estar en el futuro.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            decimal alreadyPaid = SaleFinancials.SumValidPayments(sale.Payments);
            decimal outstanding = sale.TotalSalePrice - alreadyPaid;

            if (dto.Amount > outstanding)
            {
                throw new ValidationException("PAYMENT_EXCEEDS_BALANCE", $"El pago ({dto.Amount}) excede el saldo pendiente ({outstanding}).");
            }

            Payment payment = new()
            {
                SaleId = sale.Id,
                PaymentDate = dto.PaymentDate,
                Amount = dto.Amount,
                Currency = dto.Currency,
                PaymentMethod = dto.PaymentMethod,
                Reference = dto.Reference,
                Status = PaymentStatusEnum.Completed,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Fee is not null)
            {
                payment.Fees.Add(CreateFee(payment, dto.Amount, dto.Fee));
            }

            _context.Payments.Add(payment);

            RecalculateSaleFinancials(sale);

            await _auditService.RecordAsync("Payment", payment.Id.ToString(), AuditActionEnum.Create, newValues: $"Sale={sale.SaleNumber}; Amount={payment.Amount}; Method={payment.PaymentMethod}", userId: null, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return PaymentDto.FromEntity(payment, payment.Fees.ToList());
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw new ConflictException("CONCURRENCY_CONFLICT", "Otro usuario modificó la misma venta simultáneamente. La operación fue revertida; inténtelo de nuevo.");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<PaymentDto> UpdateAsync(Guid id, UpdatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        Payment? payment = await _context.Payments
            .Include(p => p.Fees)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("PAYMENT_NOT_FOUND", "El pago no existe.");

        Sale sale = await GetSaleWithFinancialsAsync(payment.SaleId, cancellationToken);

        if (dto.Amount <= 0m)
        {
            throw new ValidationException("PAYMENT_AMOUNT_INVALID", "El monto del pago debe ser mayor que cero.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Other valid payments exclude the one being updated.
            decimal otherPayments = sale.Payments
                .Where(p => p.Id != payment.Id)
                .Where(p => SaleFinancials.CountsTowardsBalance(p.Status))
                .Sum(p => p.Amount);

            decimal outstanding = sale.TotalSalePrice - otherPayments;

            if (SaleFinancials.CountsTowardsBalance(dto.Status) && dto.Amount > outstanding)
            {
                throw new ValidationException("PAYMENT_EXCEEDS_BALANCE", $"El pago ({dto.Amount}) excede el saldo pendiente ({outstanding}).");
            }

            payment.Amount = dto.Amount;
            payment.PaymentDate = dto.PaymentDate;
            payment.PaymentMethod = dto.PaymentMethod;
            payment.Reference = dto.Reference;
            payment.Status = dto.Status;
            payment.Notes = dto.Notes;
            payment.UpdatedAt = DateTime.UtcNow;

            if (dto.Fee is not null)
            {
                _context.PaymentFees.RemoveRange(payment.Fees);
                payment.Fees.Clear();
                payment.Fees.Add(CreateFee(payment, dto.Amount, dto.Fee));
            }

            RecalculateSaleFinancials(sale);

            await _auditService.RecordAsync("Payment", payment.Id.ToString(), AuditActionEnum.Update, newValues: $"Sale={sale.SaleNumber}; Amount={payment.Amount}; Status={payment.Status}", userId: null, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return PaymentDto.FromEntity(payment, payment.Fees.ToList());
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Financial summary always recomputed from source data:
    /// PaidAmount = sum of valid payments (Pending or Completed),
    /// OutstandingBalance = TotalSalePrice - PaidAmount,
    /// TotalCost/PaymentFees/GrossProfit/ProfitMargin recalculated, never trusted from stored values.
    /// </summary>
    public async Task<SaleFinancialSummaryDto> GetFinancialSummaryAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        Sale sale = await GetSaleWithFinancialsAsync(saleId, cancellationToken);

        decimal paidAmount = SaleFinancials.SumValidPayments(sale.Payments);
        SaleFinancials.Apply(sale, _financialCalculator);

        return new SaleFinancialSummaryDto
        {
            SaleId = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleTotal = sale.TotalSalePrice,
            PaidAmount = paidAmount,
            OutstandingBalance = sale.TotalSalePrice - paidAmount,
            TotalCost = sale.TotalCost,
            PaymentFees = sale.TotalPaymentFees,
            GrossProfit = sale.GrossProfit,
            ProfitMargin = sale.ProfitMargin,
            CurrencyCode = sale.Currency.ToString()
        };
    }

    private PaymentFee CreateFee(Payment payment, decimal paymentAmount, CreatePaymentFeeDto feeDto)
    {
        decimal calculated = _feeCalculator.Calculate(paymentAmount, feeDto.FeeType, feeDto.Rate, feeDto.FixedAmount);

        return new PaymentFee
        {
            PaymentId = payment.Id,
            FeeType = feeDto.FeeType,
            Rate = feeDto.Rate,
            FixedAmount = feeDto.FixedAmount,
            CalculatedAmount = calculated,
            Currency = feeDto.Currency,
            Notes = feeDto.Notes
        };
    }

    private void RecalculateSaleFinancials(Sale sale)
    {
        decimal paidAmount = SaleFinancials.SumValidPayments(sale.Payments);
        SaleFinancials.Apply(sale, _financialCalculator);
        SaleFinancials.UpdateStatusFromPayments(sale, paidAmount);
        sale.UpdatedAt = DateTime.UtcNow;
    }

    private async Task<Sale> GetSaleWithFinancialsAsync(Guid saleId, CancellationToken cancellationToken)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .ThenInclude(p => p.Fees)
            .FirstOrDefaultAsync(s => s.Id == saleId, cancellationToken)
            ?? throw new NotFoundException("SALE_NOT_FOUND", "La venta no existe.");
    }
}
