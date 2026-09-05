using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/customers")]
[Produces("application/json")]
public class CustomersController : ApiControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    /// <summary>Paged customer list with optional search (first name, last name, email or phone).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _customerService.SearchAsync(search, page, pageSize, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _customerService.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        CustomerDto created = await _customerService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        return Ok(await _customerService.UpdateAsync(id, dto, cancellationToken));
    }

    /// <summary>
    /// Physical delete is blocked when the customer has quotes, sales, payments
    /// or other related operations (HTTP 409). Use status change instead.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
