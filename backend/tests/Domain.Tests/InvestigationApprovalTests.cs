using System;
using RuntimeInvestigation.Domain.Entities;
using Xunit;

namespace Domain.Tests;

public class InvestigationApprovalTests
{
    [Fact]
    public void NewInvestigation_HasDraftApprovalStatus()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        Assert.Equal(ApprovalStatus.Draft, inv.ApprovalStatus);
        Assert.Null(inv.ApprovedBy);
        Assert.Null(inv.ApprovedAt);
        Assert.Null(inv.RejectionReason);
    }

    [Fact]
    public void SubmitForApproval_FromDraft_TransitionsToPendingApproval()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        inv.SubmitForApproval();

        Assert.Equal(ApprovalStatus.PendingApproval, inv.ApprovalStatus);
    }

    [Fact]
    public void SubmitForApproval_FromPending_ThrowsInvalidOperationException()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        inv.SubmitForApproval();

        var ex = Assert.Throws<InvalidOperationException>(() => inv.SubmitForApproval());
        Assert.Contains("Cannot submit investigation for approval", ex.Message);
    }

    [Fact]
    public void Approve_FromPending_TransitionsToActiveAndInvestigating()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        inv.SubmitForApproval();

        inv.Approve("admin-user-42");

        Assert.Equal(ApprovalStatus.Active, inv.ApprovalStatus);
        Assert.Equal("admin-user-42", inv.ApprovedBy);
        Assert.NotNull(inv.ApprovedAt);
        Assert.Equal(InvestigationStatus.Investigating, inv.Status);
    }

    [Fact]
    public void Approve_FromDraft_ThrowsInvalidOperationException()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");

        var ex = Assert.Throws<InvalidOperationException>(() => inv.Approve("admin-user-42"));
        Assert.Contains("Cannot approve investigation in Draft status", ex.Message);
    }

    [Fact]
    public void Approve_WithoutAdminId_ThrowsArgumentException()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        inv.SubmitForApproval();

        Assert.Throws<ArgumentException>(() => inv.Approve(""));
    }

    [Fact]
    public void Reject_FromPending_TransitionsToRejectedWithReason()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        inv.SubmitForApproval();

        inv.Reject("admin-user-42", "Inappropriate probe scope");

        Assert.Equal(ApprovalStatus.Rejected, inv.ApprovalStatus);
        Assert.Equal("admin-user-42", inv.ApprovedBy);
        Assert.Equal("Inappropriate probe scope", inv.RejectionReason);
    }

    [Fact]
    public void Reject_FromDraft_ThrowsInvalidOperationException()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");

        var ex = Assert.Throws<InvalidOperationException>(() => inv.Reject("admin-user-42", "Reason"));
        Assert.Contains("Cannot reject investigation in Draft status", ex.Message);
    }

    [Fact]
    public void Resubmit_FromRejected_TransitionsBackToPendingApproval()
    {
        var inv = new Investigation("app-1", "Investigation 1", "Desc");
        inv.SubmitForApproval();
        inv.Reject("admin-user-42", "Needs refined scope");

        inv.SubmitForApproval();

        Assert.Equal(ApprovalStatus.PendingApproval, inv.ApprovalStatus);
    }
}
