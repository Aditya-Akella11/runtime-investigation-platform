using System;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Common.Interfaces;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Domain.Tests;

public class TenantIsolationTests
{
    [Fact]
    public void Investigation_HasTenantIdAndCreatedBy()
    {
        var inv = new Investigation("app-1", "Test Title", "Desc", "tenant-a", "user-1");
        Assert.Equal("tenant-a", inv.TenantId);
        Assert.Equal("user-1", inv.CreatedBy);
    }

    [Fact]
    public void RuntimeProbe_HasTenantId()
    {
        var probe = new RuntimeProbe("inv-1", ProbeType.Log, "Target.Class", null, DateTime.UtcNow.AddHours(1), null, "tenant-b");
        Assert.Equal("tenant-b", probe.TenantId);
    }

    [Fact]
    public void TenantContext_SetContext_UpdatesTenantAndRole()
    {
        var context = new TenantContext();
        context.SetContext("tenant-xyz", "user-admin", "Admin");
        Assert.Equal("tenant-xyz", context.TenantId);
        Assert.Equal("user-admin", context.UserId);
        Assert.Equal("Admin", context.Role);
    }

    [Fact]
    public async Task TenantIsolation_InvestigationRepository_FiltersByTenant()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetContext("tenant-a", "user-1", "Admin");

        var repo = new InMemoryInvestigationRepository { TenantContext = tenantContext };
        await repo.CreateAsync(new Investigation("app-1", "Tenant A Inv", null, "tenant-a"));
        await repo.CreateAsync(new Investigation("app-1", "Tenant B Inv", null, "tenant-b"));

        var listA = await repo.GetAllAsync();
        Assert.Single(listA);
        Assert.Equal("Tenant A Inv", listA[0].Title);

        tenantContext.SetContext("tenant-b", "user-2", "Admin");
        var listB = await repo.GetAllAsync();
        Assert.Single(listB);
        Assert.Equal("Tenant B Inv", listB[0].Title);
    }

    [Fact]
    public async Task TenantIsolation_ProbeRepository_FiltersByTenant()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetContext("tenant-1", "u1", "Admin");

        var repo = new InMemoryProbeRepository { TenantContext = tenantContext };
        var probe1 = new RuntimeProbe("inv-1", ProbeType.Log, "App.Method1", null, DateTime.UtcNow.AddHours(1), null, "tenant-1");
        var probe2 = new RuntimeProbe("inv-1", ProbeType.Log, "App.Method2", null, DateTime.UtcNow.AddHours(1), null, "tenant-2");

        await repo.CreateAsync(probe1);
        await repo.CreateAsync(probe2);

        var probesTenant1 = await repo.GetAllAsync();
        Assert.Single(probesTenant1);
        Assert.Equal("tenant-1", probesTenant1[0].TenantId);

        var probeByIdTenant2 = await repo.GetByIdAsync(probe2.Id);
        Assert.Null(probeByIdTenant2); // Cannot access tenant-2 probe while in tenant-1
    }

    [Fact]
    public async Task TenantIsolation_InvestigationService_HidesCrossTenantData()
    {
        var tenantContext = new TenantContext();
        tenantContext.SetContext("acme-corp", "alice", "Investigator");

        var repo = new InMemoryInvestigationRepository(); // raw repo without tenant awareness
        var service = new InvestigationService(repo, tenantContext);

        // Create under acme-corp
        var created = await service.CreateAsync(new CreateInvestigationCommand("app-1", "Acme Investigation"));
        Assert.True(created.IsSuccess);

        // Switch to different tenant
        tenantContext.SetContext("beta-corp", "bob", "Investigator");

        // Query by ID should fail with NotFound (hidden across tenant)
        var fetchResult = await service.GetByIdAsync(created.Value!.Id);
        Assert.False(fetchResult.IsSuccess);
        Assert.Equal("NotFound", fetchResult.Error?.Code);

        // GetAll should return empty
        var all = await service.GetAllAsync();
        Assert.Empty(all);
    }
}
