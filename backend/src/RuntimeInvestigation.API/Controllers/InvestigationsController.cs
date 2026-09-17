using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Features.Investigations;

using RuntimeInvestigation.Application.Features.Approval;
using RuntimeInvestigation.Application.Features.Templates;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class InvestigationsController : ControllerBase
{
    private readonly InvestigationService _service;
    private readonly ApprovalService? _approvalService;
    private readonly TemplateService? _templateService;

    public InvestigationsController(
        InvestigationService service,
        ApprovalService? approvalService = null,
        TemplateService? templateService = null)
    {
        _service = service;
        _approvalService = approvalService;
        _templateService = templateService;
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

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<InvestigationDto>> Submit(string id, CancellationToken cancellationToken)
    {
        if (_approvalService == null) return StatusCode(500, new { error = "Approval service not configured." });
        var result = await _approvalService.SubmitForApprovalAsync(new SubmitApprovalCommand(id), cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }

    [HttpPost("{id}/approve")]
    public async Task<ActionResult<InvestigationDto>> Approve(string id, [FromBody] ApproveRequest? request, CancellationToken cancellationToken)
    {
        if (_approvalService == null) return StatusCode(500, new { error = "Approval service not configured." });
        var result = await _approvalService.ApproveAsync(new ApproveInvestigationCommand(id, request?.AdminId), cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Forbidden") return StatusCode(403, new { error = result.Error.Message });
            if (result.Error?.Code == "NotFound") return NotFound(new { error = result.Error.Message });
            return BadRequest(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }

    [HttpPost("{id}/reject")]
    public async Task<ActionResult<InvestigationDto>> Reject(string id, [FromBody] RejectRequest request, CancellationToken cancellationToken)
    {
        if (_approvalService == null) return StatusCode(500, new { error = "Approval service not configured." });
        var result = await _approvalService.RejectAsync(new RejectInvestigationCommand(id, request.Reason, request.AdminId), cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "Forbidden") return StatusCode(403, new { error = result.Error.Message });
            if (result.Error?.Code == "NotFound") return NotFound(new { error = result.Error.Message });
            return BadRequest(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }

    [HttpPost("from-template")]
    public async Task<ActionResult<TemplateInstantiationResult>> CreateFromTemplate([FromBody] CreateFromTemplateCommand command, CancellationToken cancellationToken)
    {
        if (_templateService == null) return StatusCode(500, new { error = "Template service not configured." });
        var result = await _templateService.CreateFromTemplateAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }
}

public sealed record ApproveRequest(string? AdminId = null);
public sealed record RejectRequest(string Reason, string? AdminId = null);

