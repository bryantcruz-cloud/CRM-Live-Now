using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/customers/{customerId:guid}/checklist")]
[Produces("application/json")]
public class CustomerChecklistController : ApiControllerBase
{
    private readonly IChecklistService _checklistService;

    public CustomerChecklistController(IChecklistService checklistService) => _checklistService = checklistService;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ChecklistItemDto>>> GetAll(Guid customerId, CancellationToken cancellationToken = default)
        => Ok(await _checklistService.GetByCustomerAsync(customerId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ChecklistItemDto>> Create(Guid customerId, CreateChecklistItemDto dto, CancellationToken cancellationToken = default)
    {
        ChecklistItemDto created = await _checklistService.CreateAsync(customerId, dto, cancellationToken);
        return Created(string.Empty, created);
    }
}

[ApiController]
[Route("api/checklist")]
[Produces("application/json")]
public class ChecklistController : ApiControllerBase
{
    private readonly IChecklistService _checklistService;

    public ChecklistController(IChecklistService checklistService) => _checklistService = checklistService;

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChecklistItemDto>> Update(Guid id, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default)
        => Ok(await _checklistService.UpdateAsync(id, dto, cancellationToken));
}
