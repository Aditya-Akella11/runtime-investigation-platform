using System.Threading.Tasks;
using RuntimeInvestigation.Application.Common.Interfaces;
using RuntimeInvestigation.Application.Features.Approval;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Application.Tests;

public class ApprovalServiceTests
{
    private static (ApprovalService sut, InMemoryInvestigationRepository repo, TenantContext tenantContext) CreateSut(string role = "Admin", string tenantId = "tenant-1")
    {
        var tenantContext = new TenantContext();
        tenantContext.SetContext(tenantId, "admin-user-1", role);

        var repo = new InMemoryInvestigationRepository { TenantContext = tenantContext };
        var sut = new ApprovalService(repo, tenantContext);

        return (sut, repo, tenantContext);
    }

    [Fact]
    public async Task SubmitForApprovalAsync_TransitionsToPending()
    {
        var (sut, repo, _) = CreateSut(role: "Investigator");
        var inv = new Investigation("app-1", "Test Investigation", "Desc", "tenant-1", "user-1");
        await repo.CreateAsync(inv);

        var result = await sut.SubmitForApprovalAsync(new SubmitApprovalCommand(inv.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("PendingApproval", result.Value!.ApprovalStatus);
    }

    [Fact]
    public async Task ApproveAsync_NonAdmin_ReturnsForbidden()
    {
        var (sut, repo, _) = CreateSut(role: "Investigator");
        var inv = new Investigation("app-1", "Test Investigation", "Desc", "tenant-1", "user-1");
        inv.SubmitForApproval();
        await repo.CreateAsync(inv);

        var result = await sut.ApproveAsync(new ApproveInvestigationCommand(inv.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error?.Code);
    }

    [Fact]
    public async Task ApproveAsync_Admin_SucceedsAndTransitionsToActive()
    {
        var (sut, repo, _) = CreateSut(role: "Admin");
        var inv = new Investigation("app-1", "Test Investigation", "Desc", "tenant-1", "user-1");
        inv.SubmitForApproval();
        await repo.CreateAsync(inv);

        var result = await sut.ApproveAsync(new ApproveInvestigationCommand(inv.Id));

        Assert.True(result.IsSuccess);
        Assert.Equal("Active", result.Value!.ApprovalStatus);
        Assert.Equal("admin-user-1", result.Value!.ApprovedBy);
        Assert.NotNull(result.Value!.ApprovedAt);
        Assert.Equal("Investigating", result.Value!.Status);
    }

    [Fact]
    public async Task ApproveAsync_FromDraft_ReturnsInvalidTransition()
    {
        var (sut, repo, _) = CreateSut(role: "Admin");
        var inv = new Investigation("app-1", "Test Investigation", "Desc", "tenant-1", "user-1");
        await repo.CreateAsync(inv);

        var result = await sut.ApproveAsync(new ApproveInvestigationCommand(inv.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("InvalidTransition", result.Error?.Code);
    }

    [Fact]
    public async Task RejectAsync_NonAdmin_ReturnsForbidden()
    {
        var (sut, repo, _) = CreateSut(role: "Viewer");
        var inv = new Investigation("app-1", "Test Investigation", "Desc", "tenant-1", "user-1");
        inv.SubmitForApproval();
        await repo.CreateAsync(inv);

        var result = await sut.RejectAsync(new RejectInvestigationCommand(inv.Id, "Rejected"));

        Assert.False(result.IsSuccess);
        Assert.Equal("Forbidden", result.Error?.Code);
    }

    [Fact]
    public async Task RejectAsync_Admin_SucceedsAndStoresReason()
    {
        var (sut, repo, _) = CreateSut(role: "Admin");
        var inv = new Investigation("app-1", "Test Investigation", "Desc", "tenant-1", "user-1");
        inv.SubmitForApproval();
        await repo.CreateAsync(inv);

        var result = await sut.RejectAsync(new RejectInvestigationCommand(inv.Id, "Target class is out of bounds"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Rejected", result.Value!.ApprovalStatus);
        Assert.Equal("Target class is out of bounds", result.Value!.RejectionReason);
    }

    [Fact]
    public async Task Approval_TenantIsolation_CannotApproveOtherTenantInvestigation()
    {
        var (sut, repo, tenantContext) = CreateSut(role: "Admin", tenantId: "tenant-a");
        var invB = new Investigation("app-2", "Other Investigation", "Desc", "tenant-b", "user-2");
        invB.SubmitForApproval();
        await repo.CreateAsync(invB);

        var result = await sut.ApproveAsync(new ApproveInvestigationCommand(invB.Id));

        Assert.False(result.IsSuccess);
        Assert.Equal("NotFound", result.Error?.Code);
    }
}
