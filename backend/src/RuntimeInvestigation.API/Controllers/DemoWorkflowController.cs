using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RuntimeInvestigation.API.Services;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DemoWorkflowController : ControllerBase
{
    private readonly DemoWorkflowStore _store;
    private readonly IProbeDispatcher _dispatcher;

    public DemoWorkflowController(DemoWorkflowStore store, IProbeDispatcher dispatcher)
    {
        _store = store;
        _dispatcher = dispatcher;
    }

    [HttpGet("investigations")]
    public ActionResult<IReadOnlyCollection<InvestigationRecord>> GetInvestigations() => Ok(_store.GetInvestigations());

    [HttpPost("investigations")]
    public ActionResult<InvestigationRecord> CreateInvestigation([FromBody] CreateInvestigationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Application) || string.IsNullOrWhiteSpace(request.Environment))
        {
            return BadRequest("Title, application, and environment are required.");
        }

        return Ok(_store.CreateInvestigation(request.Title, request.Application, request.Environment, request.Description ?? string.Empty));
    }

    [HttpGet("probes")]
    public ActionResult<IReadOnlyCollection<RuntimeProbeRecord>> GetProbes() => Ok(_store.GetProbes());

    [HttpPost("probes")]
    public ActionResult<RuntimeProbeRecord> CreateProbe([FromBody] CreateProbeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InvestigationId) || string.IsNullOrWhiteSpace(request.Application) || string.IsNullOrWhiteSpace(request.TargetClass) || string.IsNullOrWhiteSpace(request.TargetMethod))
        {
            return BadRequest("Investigation, application, target class, and target method are required.");
        }

        return Ok(_store.CreateProbe(request));
    }

    [HttpPost("probes/{id}/deploy")]
    public async Task<ActionResult<RuntimeProbeRecord>> DeployProbe(string id)
    {
        var probe = _store.DeployProbe(id);
        if (probe is not null)
        {
            await _dispatcher.DeployAsync(probe.Id, probe.Application, probe.TargetClass, probe.TargetMethod, DateTimeOffset.UtcNow.AddMinutes(probe.DurationMinutes));
        }
        return probe is null ? NotFound() : Ok(probe);
    }

    [HttpPost("probes/{id}/remove")]
    public async Task<ActionResult<RuntimeProbeRecord>> RemoveProbe(string id)
    {
        var probe = _store.RemoveProbe(id);
        if (probe is not null)
        {
            await _dispatcher.RemoveAsync(probe.Id, "Removed by operator");
        }
        return probe is null ? NotFound() : Ok(probe);
    }

    [HttpPost("probes/{id}/expire")]
    public ActionResult<RuntimeProbeRecord> ExpireProbe(string id)
    {
        var probe = _store.ExpireProbe(id);
        return probe is null ? NotFound() : Ok(probe);
    }

    [HttpGet("evidence")]
    public ActionResult<IReadOnlyCollection<EvidenceRecord>> GetEvidence([FromQuery] string? probeId) =>
        Ok(string.IsNullOrWhiteSpace(probeId) ? _store.GetEvidence() : _store.GetEvidenceByProbe(probeId));

    [HttpGet("audit")]
    public ActionResult<IReadOnlyCollection<AuditEntry>> GetAudit([FromQuery] string? subjectId) =>
        Ok(string.IsNullOrWhiteSpace(subjectId) ? _store.GetAudit() : _store.GetAuditBySubject(subjectId));

    [HttpPost("reset")]
    public ActionResult ResetDemo()
    {
        _store.Reset();
        return Ok(new { message = "Demo workflow reset to initial state with sample data." });
    }
}

public sealed record CreateInvestigationRequest(string Title, string Application, string Environment, string? Description);
