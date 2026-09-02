using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Route("api")]
public sealed class ProbesController : ControllerBase
{
    private readonly ProbeService _probeService;
    private readonly IProbeResultRepository _resultRepository;

    public ProbesController(ProbeService probeService, IProbeResultRepository resultRepository)
    {
        _probeService = probeService;
        _resultRepository = resultRepository;
    }

    [HttpGet("investigations/{investigationId}/probes")]
    public async Task<ActionResult<IReadOnlyList<ProbeDto>>> GetByInvestigation(string investigationId, CancellationToken cancellationToken)
    {
        var probes = await _probeService.GetByInvestigationIdAsync(investigationId, cancellationToken);
        return Ok(probes);
    }

    [HttpGet("probes/{id}")]
    public async Task<ActionResult<ProbeDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _probeService.GetByIdAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }

    [HttpPost("investigations/{investigationId}/probes")]
    public async Task<ActionResult<ProbeDto>> AddProbe(string investigationId, [FromBody] AddProbeRequest request, CancellationToken cancellationToken)
    {
        var command = new AddProbeCommand(
            investigationId,
            request.ProbeType ?? "Log",
            request.TargetClass,
            request.TargetMethod,
            request.Expression,
            request.DurationMinutes <= 0 ? 30 : request.DurationMinutes);

        var result = await _probeService.AddProbeAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPost("probes/{id}/activate")]
    public async Task<ActionResult<ProbeDto>> Activate(string id, [FromQuery] string? application, CancellationToken cancellationToken)
    {
        var result = await _probeService.ActivateProbeAsync(id, application ?? "DefaultApp", cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }

    [HttpPost("probes/{id}/deactivate")]
    public async Task<ActionResult<ProbeDto>> Deactivate(string id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        var result = await _probeService.DeactivateProbeAsync(id, reason ?? "Operator deactivated", cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Message });
        }
        return Ok(result.Value);
    }

    [HttpGet("probes/{id}/results")]
    public async Task<ActionResult<IReadOnlyList<ProbeResultDto>>> GetResults(string id, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        var results = await _probeService.GetResultsAsync(id, limit, cancellationToken);
        return Ok(results);
    }

    [HttpPost("probes/{id}/evidence")]
    public async Task<IActionResult> IngestEvidence(string id, [FromBody] IReadOnlyList<ProbeEvidenceRecord> records, CancellationToken cancellationToken)
    {
        if (records == null || records.Count == 0)
        {
            return Ok();
        }

        var results = records.Select(r => new ProbeResult(
            id,
            r.CorrelationId,
            r.TargetClass ?? "Unknown",
            r.Values,
            null,
            0.0,
            r.CapturedAtUtc.UtcDateTime
        )).ToArray();

        await _resultRepository.AddRangeAsync(results, cancellationToken);
        return Ok(new { count = results.Length });
    }
}

public sealed record AddProbeRequest(
    string? ProbeType,
    string TargetClass,
    string TargetMethod,
    string? Expression = null,
    int DurationMinutes = 30);
