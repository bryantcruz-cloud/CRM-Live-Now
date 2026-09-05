using LiveNow.CRM.Core.Common;
using LiveNow.CRM.Core.DTOs;
using LiveNow.CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiveNow.CRM.API.Controllers;

[ApiController]
[Route("api/registrations")]
[Produces("application/json")]
public class RegistrationsController : ApiControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationsController(IRegistrationService registrationService) => _registrationService = registrationService;

    [HttpGet]
    public async Task<ActionResult<PagedResult<RegistrationDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _registrationService.GetAllAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RegistrationDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        => Ok(await _registrationService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RegistrationDto>> Create(CreateRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        RegistrationDto created = await _registrationService.CreateAsync(dto, cancellationToken);
        return Created(string.Empty, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RegistrationDto>> Update(Guid id, UpdateRegistrationDto dto, CancellationToken cancellationToken = default)
        => Ok(await _registrationService.UpdateAsync(id, dto, cancellationToken));
}
