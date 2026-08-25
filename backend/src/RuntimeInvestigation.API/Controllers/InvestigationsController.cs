using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Features.Investigations;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class InvestigationsController : ControllerBase
{
    private readonly InvestigationService _service;

    public InvestigationsController(InvestigationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvestigationDto>>> GetAll([FromQuery] string? applicationId, CancellationToken cancellationToken)
    {
        var investigations = string.IsNullOrWhiteSpace(applicationId)
            ? await _service.GetAllAsync(cancellationToken)
            : await _service.GetByApplicationIdAsync(applicationId, cancellationToken);

        return Ok(investigations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvestigationDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<InvestigationDto>> Create([FromBody] CreateInvestigationCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<InvestigationDto>> Update(string id, [FromBody] UpdateInvestigationCommand command, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/start")]
    public async Task<ActionResult<InvestigationDto>> Start(string id, CancellationToken cancellationToken)
    {
        var result = await _service.StartAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/resolve")]
    public async Task<ActionResult<InvestigationDto>> Resolve(string id, CancellationToken cancellationToken)
    {
        var result = await _service.ResolveAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    [HttpPost("{id}/close")]
    public async Task<ActionResult<InvestigationDto>> Close(string id, CancellationToken cancellationToken)
    {
        var result = await _service.CloseAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error?.Message });
        }

        return NoContent();
    }
}
