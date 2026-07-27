using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Features.Applications;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
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
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}

public sealed record CreateApplicationRequest(string Name, string? Description);
