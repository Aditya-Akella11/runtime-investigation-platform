using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Features.Templates;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TemplatesController : ControllerBase
{
    private readonly TemplateService _templateService;

    public TemplatesController(TemplateService templateService)
    {
        _templateService = templateService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvestigationTemplateDto>>> GetAll(CancellationToken cancellationToken)
    {
        var templates = await _templateService.GetTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvestigationTemplateDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var template = await _templateService.GetTemplateByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return NotFound(new { error = $"Template '{id}' not found." });
        }

        return Ok(template);
    }

    [HttpPost("instantiate")]
    public async Task<ActionResult<TemplateInstantiationResult>> Instantiate([FromBody] CreateFromTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await _templateService.CreateFromTemplateAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/instantiate")]
    public async Task<ActionResult<TemplateInstantiationResult>> InstantiateById(string id, [FromBody] InstantiateTemplateRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateFromTemplateCommand(id, request.ApplicationId, request.Title, request.Description);
        var result = await _templateService.CreateFromTemplateAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }
}

public sealed record InstantiateTemplateRequest(string ApplicationId, string? Title = null, string? Description = null);
