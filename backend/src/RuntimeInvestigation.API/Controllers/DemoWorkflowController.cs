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

    public DemoWorkflowController(DemoWorkflowStore store)
    {
        _store = store;
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
    public ActionResult<RuntimeProbeRecord> DeployProbe(string id)
    {
        var probe = _store.DeployProbe(id);
        return probe is null ? NotFound() : Ok(probe);
    }

    [HttpPost("probes/{id}/remove")]
    public ActionResult<RuntimeProbeRecord> RemoveProbe(string id)
    {
        var probe = _store.RemoveProbe(id);
        return probe is null ? NotFound() : Ok(probe);
    }

    [HttpGet("evidence")]
    public ActionResult<IReadOnlyCollection<EvidenceRecord>> GetEvidence() => Ok(_store.GetEvidence());

    [HttpGet("audit")]
    public ActionResult<IReadOnlyCollection<AuditEntry>> GetAudit() => Ok(_store.GetAudit());
}

public sealed record CreateInvestigationRequest(string Title, string Application, string Environment, string? Description);
