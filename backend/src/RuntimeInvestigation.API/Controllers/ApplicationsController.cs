using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Features.Applications;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ApplicationsController : ControllerBase
{
    private readonly ApplicationService _applicationService;

    public ApplicationsController(ApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var applications = await _applicationService.GetAllAsync(cancellationToken);
        return Ok(applications);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Create([FromBody] CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Application name is required.");
        }

        var created = await _applicationService.CreateAsync(request.Name, request.Description, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApplicationDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var application = await _applicationService.GetByIdAsync(id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApplicationDto>> Update(string id, [FromBody] UpdateApplicationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Application name is required.");
        var updated = await _applicationService.UpdateAsync(id, request.Name, request.Description, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken) =>
        await _applicationService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}

public sealed record CreateApplicationRequest(string Name, string? Description);
public sealed record UpdateApplicationRequest(string Name, string? Description);
