using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Entities;
using LiveNow.CRM.Core.Enums;
using LiveNow.CRM.Core.Interfaces;
using LiveNow.CRM.Core.Interfaces.Services;
using LiveNow.CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LiveNow.CRM.API.Services;

public class CustomerService : ICustomerService
{
    private readonly LiveNowDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public CustomerService(
        LiveNowDbContext context,
        IUnitOfWork unitOfWork,
        IAuditService auditService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<PagedResult<CustomerDto>> SearchAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        int safePage = Math.Max(page, 1);
        int safePageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100);

        IQueryable<Customer> query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.ToLower().Contains(term)));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<Customer> items = await query
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<CustomerDto>.Create(
            items.Select(CustomerDto.FromEntity).ToList(),
            safePage,
            safePageSize,
            totalCount);
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Customer? customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");

        return CustomerDto.FromEntity(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto.FirstName, dto.LastName, dto.Email);

        if (await _context.Customers.AnyAsync(c => c.Email.ToLower() == dto.Email.Trim().ToLower(), cancellationToken))
        {
            throw new ConflictException("CUSTOMER_EMAIL_EXISTS", "Ya existe un cliente con ese email.");
        }

        Customer customer = new()
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim(),
            Phone = dto.Phone,
            Country = dto.Country,
            City = dto.City,
            DateOfBirth = dto.DateOfBirth,
            Notes = dto.Notes,
            Status = CustomerStatusEnum.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Customers.Add(customer);
        await _auditService.RecordAsync("Customer", customer.Id.ToString(), AuditActionEnum.Create, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerDto.FromEntity(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        Validate(dto.FirstName, dto.LastName, dto.Email);

        Customer? customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");

        customer.FirstName = dto.FirstName.Trim();
        customer.LastName = dto.LastName.Trim();
        customer.Email = dto.Email.Trim();
        customer.Phone = dto.Phone;
        customer.Country = dto.Country;
        customer.City = dto.City;
        customer.DateOfBirth = dto.DateOfBirth;
        customer.Notes = dto.Notes;
        customer.Status = dto.Status;
        customer.UpdatedAt = DateTime.UtcNow;

        await _auditService.RecordAsync("Customer", customer.Id.ToString(), AuditActionEnum.Update, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CustomerDto.FromEntity(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Customer? customer = await _context.Customers
            .Include(c => c.Quotes)
            .Include(c => c.Sales)
            .Include(c => c.HotelReservations)
            .Include(c => c.RunnerRegistrations)
            .Include(c => c.Checklists)
            .Include(c => c.Cancellations)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("CUSTOMER_NOT_FOUND", "El cliente no existe.");

        bool hasRelatedOperations =
            customer.Quotes.Count > 0 ||
            customer.Sales.Count > 0 ||
            customer.HotelReservations.Count > 0 ||
            customer.RunnerRegistrations.Count > 0 ||
            customer.Checklists.Count > 0 ||
            customer.Cancellations.Count > 0;

        if (hasRelatedOperations)
        {
            // Logical deactivation: financial/operational data must never be physically deleted.
            customer.IsActive = false;
            customer.Status = CustomerStatusEnum.Inactive;
            customer.UpdatedAt = DateTime.UtcNow;
            await _auditService.RecordAsync("Customer", customer.Id.ToString(), AuditActionEnum.Delete, userId: null, cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        _context.Customers.Remove(customer);
        await _auditService.RecordAsync("Customer", customer.Id.ToString(), AuditActionEnum.Delete, userId: null, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ValidationException("CUSTOMER_FIRST_NAME_REQUIRED", "El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new ValidationException("CUSTOMER_LAST_NAME_REQUIRED", "El apellido es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new ValidationException("CUSTOMER_EMAIL_INVALID", "El email es obligatorio y debe ser válido.");
        }
    }
}