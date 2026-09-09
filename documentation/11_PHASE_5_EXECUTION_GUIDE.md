# Phase 5 Execution Guide: Enterprise Scalability (8-10 weeks)

## Overview
You completed Phase 4 with a working POC. Phase 5 transforms that into an **enterprise-ready SaaS platform** that customers will pay for.

**Phase 5 Goals:**
1. ✅ Multi-tenant isolation (Company A can't see Company B data)
2. ✅ Advanced probe types (snapshots, metrics, conditions)
3. ✅ Investigation templates (pre-built for common scenarios)
4. ✅ Kubernetes support (modern deployment)
5. ✅ Enterprise workflows (approval, RBAC)
6. ✅ Analytics & monitoring (prove ROI to customers)
7. ✅ Self-service onboarding (reduce sales friction)

---

## Critical Path: Start Here

**If you have 1 week to ship:** Do these in order, skip the rest:
1. **Sprint 8 (Multi-tenancy)** — Tickets #63-69 (28 pts, ~1 week)
   - Without this, you can't bill multiple customers
   - Highest ROI

Then pick ONE more:
- **Sprint 9 (Templates)** — If customers want faster onboarding
- **Sprint 11 (Monitoring)** — If customers need observability
- **Sprint 12 (Operations)** — If you're doing customer sales calls

**If you have 10 weeks:** Do all 6 sprints sequentially.

---

## Execution Strategy

### For Solo Founder (No team):
```
Week 1: Sprint 7 (Advanced Probes) 
Week 2: Sprint 8 (Multi-tenancy) ← HARDEST, most rewarding
Week 3: Sprint 9 (Templates)
Week 4: Sprint 10 (Kubernetes) ← Can skip if no K8s customers
Week 5: Sprint 11 (Monitoring)
Week 6: Sprint 12 (Operations)
```

**Weekly rhythm:**
- Monday-Tuesday: Core feature implementation (tickets 1-3)
- Wednesday: Integration/testing (verify tickets work together)
- Thursday: Frontend polish
- Friday: Demo, bug fixes, prepare for next week

### For 2-Person Team:
```
Person A (Backend): Sprints 7, 8, 10, 11
Person B (Frontend): Sprints 9, 11, 12
Merge at end of each sprint
```

### For 3+ Person Team:
```
Parallel:
- Team A: Sprint 7 + Sprint 8 (core features)
- Team B: Sprint 9 + Sprint 10 (user features + ops)
- Team C: Sprint 11 + Sprint 12 (monitoring + support)
```

---

## Pre-Sprint Checklist

Before starting Phase 5, verify Phase 4 is truly complete:

```bash
# Run ALL Phase 4 tests
dotnet test backend/tests/ --filter "Category=Phase4" 

# Verify docker-compose stack works
docker-compose up -d
# Check:
# - API responds: curl http://localhost:5000/health
# - Frontend loads: http://localhost:4200
# - Agent running: curl http://localhost:5001/health

# Verify probe works end-to-end
# 1. Create investigation via UI
# 2. Add log probe
# 3. Activate (target SimpleApp)
# 4. See logs appear in UI
```

If any step fails, fix Phase 4 first. Don't start Phase 5 with broken foundation.

---

## Sprint 7: Advanced Probes (24 pts, ~1 week)

**Goal:** Support Log, Snapshot, Metric probe types

**Priority:** HIGH — Customers will ask for these immediately

**Key tickets:**
- #56-57: SnapshotProbe (capture variables)
- #58-59: MetricProbe (performance counters)
- #60: Conditions (if-based filtering)

**Daily workflow:**
```
Day 1-2:   Implement SnapshotProbe IL injection (#57)
Day 3:     Write tests, verify snapshot captures work
Day 4:     Implement MetricProbe (#58-59)
Day 5:     Add condition evaluator (#60), UI updates
           Test: create all 3 probe types, activate, verify
```

**Acceptance criteria (end of sprint):**
- [ ] Can create Log, Snapshot, Metric probes from UI
- [ ] Snapshot probes capture variables correctly
- [ ] Metric probes track call count, duration, exceptions
- [ ] Conditions work: "args[0] > 100" filters captures
- [ ] All 7 tickets passing tests
- [ ] docker-compose up → full stack works

**Gotchas:**
- IL injection for snapshots is complex (boxing, variable resolution)
- Metric overhead must stay <2% or customers complain
- Condition parsing can get tricky (start simple: just args[n] > value)

---

## Sprint 8: Multi-Tenancy & RBAC (28 pts, ~1-2 weeks)

**Goal:** Isolate data per customer, enforce role-based permissions

**Priority:** CRITICAL — This is what makes you a SaaS platform, not a tool

**Key tickets:**
- #63-65: Tenant isolation (add TenantId to all entities)
- #66-67: Users & roles (Admin, Investigator, Viewer)
- #68-69: User management UI

**Daily workflow:**
```
Day 1-2:   Add TenantId to ALL domain entities (#63)
           Update all repositories to filter by TenantId (#65)
           
Day 3:     Implement TenantContextMiddleware (#64)
           Test: Query for "user in tenant A" returns nothing for tenant B
           
Day 4:     Create User/Role entities (#66)
           Add [Authorize] attributes (#67)
           
Day 5:     User management endpoints (#68)
           Frontend: user settings page (#69)
           
Day 6:     Test: Create 2 tenants, verify data isolation
           Test: Switch roles, verify permissions enforced
```

**Acceptance criteria (end of sprint):**
- [ ] All entities have TenantId
- [ ] Repositories filter by TenantId automatically
- [ ] Can create new tenant via signup
- [ ] Users have roles: Admin, Investigator, Viewer
- [ ] [Authorize] blocks unauthorized access (403 Forbidden)
- [ ] Admin can invite users, change roles
- [ ] Each tenant sees only own data
- [ ] All 8 tickets passing tests

**Critical tests to write:**
```csharp
[TestMethod]
public void GetInvestigation_FromDifferentTenant_ReturnsNotFound()
{
    var inv = CreateInvestigationForTenantA();
    var result = GetInvestigationAsUserFromTenantB(inv.Id);
    
    Assert.AreEqual(404, result.StatusCode);
}

[TestMethod]
public void CreateProbe_WithViewerRole_ReturnsForbidden()
{
    var user = CreateUserWithRole(UserRole.Viewer);
    var result = CreateProbeAs(user);
    
    Assert.AreEqual(403, result.StatusCode);
}
```

**Gotchas:**
- Add TenantId to EVERY repository query, or you'll leak data
- Don't forget audit log (who did what) for compliance
- JWT claims must include tenant_id
- Test with multiple tenants simultaneously

---

## Sprint 9: Templates & Workflows (24 pts, ~1 week)

**Goal:** Pre-built investigation templates + approval workflow

**Priority:** MEDIUM-HIGH — Customers love templates, approval is enterprise-required

**Key tickets:**
- #70-72: InvestigationTemplate entity + API
- #73-75: Approval workflow (Draft → Pending → Active)

**Template examples to build:**
- "Debug N+1 Queries": 3 probes on ORM calls
- "Authentication Failures": probes on auth methods
- "Memory Leak": snapshot probe on allocation methods

**Daily workflow:**
```
Day 1:     Create InvestigationTemplate entity (#70)
           Seed database with 3 example templates
           
Day 2:     Implement "create from template" service (#71)
           API endpoint for /templates, /from-template (#72)
           
Day 3:     Add approval workflow fields (#73)
           Approval endpoints: approve, reject (#74)
           
Day 4:     Frontend: template selector (#75)
           Frontend: admin approval dashboard
           
Day 5:     Test: create investigation from template
           Test: workflow Draft → Pending → Active → Closed
```

**Acceptance criteria (end of sprint):**
- [ ] 3+ template examples in database
- [ ] Can list templates from UI
- [ ] Can create investigation from template (auto-populates probes)
- [ ] Approval workflow: Draft → Pending → Active
- [ ] Only Admin can approve
- [ ] Audit trail of approvals
- [ ] All 7 tickets passing tests

**Gotchas:**
- Approval workflow is a state machine (easy to get wrong)
- Conditions from template need to be copied, not shared
- Track who approved and when (audit compliance)

---

## Sprint 10: Kubernetes (28 pts, ~2 weeks)

**Goal:** Kubernetes-native agent deployment + agent discovery

**Priority:** MEDIUM — Nice-to-have, but many enterprise customers use K8s

**Key tickets:**
- #76-78: Container + K8s manifests
- #79-81: Agent discovery + routing + Helm chart

**Daily workflow:**
```
Day 1-2:   Containerize agent (#76), build Docker image
           Write K8s YAML manifests (#77)
           
Day 3-4:   Implement agent heartbeat + discovery (#78)
           Agents register, send heartbeats every 10s
           
Day 5:     Probe router: select agent for each probe (#79-80)
           Integration test: activate probe on specific agent
           
Day 6-7:   Create Helm chart (#81)
           Write K8s deployment guide (#82)
           
Day 8:     Test: helm install rip-agent → agents register + healthy
```

**Acceptance criteria (end of sprint):**
- [ ] Docker image builds, runs standalone
- [ ] K8s Deployment manifest valid
- [ ] Agents register on startup + send heartbeats
- [ ] API lists healthy agents
- [ ] Probe router selects correct agent
- [ ] Helm chart valid, installs cleanly
- [ ] Customers can: helm install → agents running
- [ ] All 8 tickets passing tests

**Gotchas:**
- Kubernetes requires privileged container (profiler needs kernel access)
- Inter-pod communication (agents in one pod, target app in another)
- Agent discovery in clustered setup (across nodes)

---

## Sprint 11: Monitoring (28 pts, ~1 week)

**Goal:** Analytics dashboard + platform observability

**Priority:** MEDIUM — Customers want to see ROI, you need to monitor your own platform

**Key tickets:**
- #83-84: Analytics entity + aggregation job
- #85-87: Dashboard, Prometheus metrics
- #88: Sample Grafana dashboard

**Daily workflow:**
```
Day 1:     Analytics entity + daily aggregation job (#83-84)
           Job runs nightly: count probes, investigations, exceptions
           
Day 2:     Analytics API endpoint (#85)
           GET /analytics?from=2024-01-01&to=2024-01-31
           
Day 3:     Frontend analytics dashboard component (#86)
           Line charts: probes over time, investigations, errors
           
Day 4:     Add Prometheus /metrics endpoint (#87)
           Expose API request counts, errors, latencies
           
Day 5:     Create sample Grafana dashboard JSON (#88)
           Customers can import: docker run grafana → import dashboard
```

**Acceptance criteria (end of sprint):**
- [ ] Analytics aggregated daily for all tenants
- [ ] Dashboard shows: probes created, active, avg overhead
- [ ] Prometheus /metrics endpoint works
- [ ] Grafana can scrape your platform
- [ ] Sample dashboard imported successfully
- [ ] All 8 tickets passing tests

**Gotchas:**
- Time-series data can get large (archive old analytics)
- Prometheus scrape interval (default 15s, may need to adjust)
- Grafana dashboard panels must reference correct metric names

---

## Sprint 12: Operations (28 pts, ~1 week)

**Goal:** Self-service signup, onboarding, support, troubleshooting

**Priority:** MEDIUM-HIGH — Reduces support load, enables self-serve sales

**Key tickets:**
- #89: Self-service signup
- #90: Onboarding checklist
- #91-92: Support tickets + troubleshooting guide
- #93-95: Health checks, KB, release checklist

**Daily workflow:**
```
Day 1:     Signup flow: create tenant + admin user (#89)
           Send welcome email with API key
           
Day 2:     Onboarding checklist component (#90)
           Shows: deploy agent → register app → create probe → activate
           
Day 3:     Support ticket system (#91)
           Users submit tickets from UI → email to support
           
Day 4:     Write troubleshooting guide (#92)
           FAQ: agent won't attach, no logs, high overhead
           
Day 5:     Probe health monitor (#93)
           Alerts if probe inactive >5 minutes
           KB article system (#94)
           Release checklist (#95)
```

**Acceptance criteria (end of sprint):**
- [ ] New users can self-signup
- [ ] Onboarding checklist appears post-signup
- [ ] Can submit support tickets from UI
- [ ] Troubleshooting guide covers main issues
- [ ] Probe health monitor runs, detects stale probes
- [ ] KB searchable
- [ ] All 7 tickets passing tests

**Gotchas:**
- Stripe/payment integration (defer to Phase 6)
- Email delivery (may fail in dev, use mock in tests)
- Health checks need to avoid false positives (network latency)

---

## Week-by-Week Checklist Template

```markdown
## Week X: [Sprint Name]

### Monday
- [ ] Read all sprint tickets
- [ ] Identify blockers
- [ ] Order tickets by dependency
- [ ] Start Ticket #NNN

### Wednesday
- [ ] Core implementation done
- [ ] Tests passing
- [ ] Integration test written

### Friday
- [ ] Demo: screenshot/video of working feature
- [ ] All sprint tickets closed
- [ ] Code reviewed (if team)
- [ ] Plan next sprint

### Blockers discovered:
- None yet

### Notes:
```

---

## Testing Strategy by Sprint

### Sprint 7: Advanced Probes
```bash
# Unit tests: IL generation, condition parsing
dotnet test backend/tests/RuntimeAgent.Tests/ILSnapshotGeneratorTests.cs
dotnet test backend/tests/Domain.Tests/ConditionEvaluatorTests.cs

# Integration test: end-to-end probe activation
dotnet test backend/tests/API.Tests/AdvancedProbeTests.cs

# Manual: UI → select probe type → verify captures work
# Run locally: docker-compose up, create each probe type
```

### Sprint 8: Multi-Tenancy
```bash
# Data isolation tests
dotnet test backend/tests/Domain.Tests/TenantIsolationTests.cs

# RBAC tests
dotnet test backend/tests/API.Tests/AuthorizationTests.cs

# Manual: Create 2 tenants, verify no data leakage
```

### Sprint 9: Templates
```bash
# State machine tests
dotnet test backend/tests/Domain.Tests/InvestigationApprovalTests.cs

# Template creation tests
dotnet test backend/tests/Application.Tests/TemplateServiceTests.cs

# Manual: Create from template → test approval flow
```

### Sprint 10: Kubernetes
```bash
# Agent discovery tests
dotnet test backend/tests/Infrastructure.Tests/AgentDiscoveryTests.cs

# Routing tests
dotnet test backend/tests/Application.Tests/ProbeRouterTests.cs

# Integration test with K8s mocks
dotnet test backend/tests/Integration.Tests/KubernetesTests.cs

# Manual: docker-compose up k8s cluster, test agent registration
```

### Sprint 11: Monitoring
```bash
# Analytics aggregation tests
dotnet test backend/tests/Application.Tests/AnalyticsAggregationTests.cs

# Prometheus scrape test
curl http://localhost:9090/metrics | grep runtime_investigation

# Manual: verify Grafana can import dashboard, panels show data
```

### Sprint 12: Operations
```bash
# Signup flow test
dotnet test backend/tests/API.Tests/SignupTests.cs

# Health monitor tests
dotnet test backend/tests/Application.Tests/ProbeHealthMonitorTests.cs

# Manual: signup as new customer → see onboarding → complete
```

---

## Common Problems & Solutions

### Problem: "TenantId filter missing, data leaked to other tenant"
**Solution:** Add unit test that verifies data isolation
```csharp
[TestMethod]
public void ListInvestigations_AsUserTenantB_ExcludesTenantAData()
{
    var tenantA = CreateTenant();
    var tenantB = CreateTenant();
    var user = CreateUserInTenant(tenantB);
    
    CreateInvestigationInTenant(tenantA);
    var result = ListInvestigationsAs(user);
    
    Assert.AreEqual(0, result.Count, "User B saw data from tenant A!");
}
```

### Problem: "Kubernetes deployment fails, agents don't register"
**Solution:** Check logs
```bash
kubectl logs -f deployment/rip-agent-0
# Look for: "Successfully registered", "Heartbeat sent"

# If not heartbeating:
kubectl exec -it deployment/rip-agent-0 -- curl http://localhost:5001/health
```

### Problem: "Approval workflow broken, can't transition states"
**Solution:** Test state machine explicitly
```csharp
[TestMethod]
public void ApprovalWorkflow_CompleteFlow()
{
    var inv = new Investigation { Status = Draft };
    Assert.IsTrue(inv.CanTransitionTo(PendingApproval)); // Draft → Pending OK
    
    inv.Status = PendingApproval;
    Assert.IsTrue(inv.CanTransitionTo(Active));  // Pending → Active OK
    Assert.IsFalse(inv.CanTransitionTo(Draft));  // Can't go back to Draft
}
```

### Problem: "Templates creating probes with wrong values"
**Solution:** Deep copy probe settings, don't reference same object
```csharp
// Wrong:
foreach (var probeTemplate in template.ProbeTemplates) {
    investigation.Probes.Add(probeTemplate); // ❌ Shared reference
}

// Right:
foreach (var probeTemplate in template.ProbeTemplates) {
    var probe = new LogProbe {
        MethodFilter = probeTemplate.MethodFilter, // ✓ Copy values
        Conditions = probeTemplate.Conditions.ToList() // ✓ Copy list
    };
    investigation.Probes.Add(probe);
}
```

### Problem: "Prometheus metrics not showing up"
**Solution:** Verify middleware is registered
```csharp
// In Program.cs, order matters:
app.UseRouting();
app.UsePrometheusMiddleware();  // Add before MapControllers
app.MapControllers();
```

---

## Git Workflow per Sprint

```bash
# Start sprint
git checkout -b sprint-7-advanced-probes

# Throughout week
git commit -m "Ticket #56: Create SnapshotProbe entity"
git commit -m "Ticket #57: IL snapshot collection"
git commit -m "Ticket #58: MetricProbe entity"

# End of sprint
git push origin sprint-7-advanced-probes
# Create PR, review, merge to main

# Next sprint
git checkout main
git pull
git checkout -b sprint-8-multi-tenancy
```

---

## Success Metrics (End of Phase 5)

- ✅ 45 tickets closed (100%)
- ✅ All tests passing (>80% coverage)
- ✅ Multi-tenant isolation verified
- ✅ Can deploy to Kubernetes
- ✅ Customers can self-signup
- ✅ Platform monitoring in place
- ✅ Support system operational

---

## What NOT to do

❌ Skip Sprint 8 (multi-tenancy) — You can't be a SaaS without it
❌ Build approval workflow without tests — State machines break easily
❌ Deploy to production without analytics running — Can't monitor yourself
❌ Postpone K8s until "later" — Customers ask for it immediately
❌ Skip documentation (troubleshooting, deployment guides) — Support costs skyrocket

---

## Success Criteria: Ready for Beta Customers

When you complete Phase 5, you should be able to say:

**"We can spin up a new customer in <5 minutes, with:**
- **Isolated data** (multi-tenant)
- **Role-based permissions** (admin, investigator, viewer)
- **Pre-built templates** (common investigation scenarios)
- **Kubernetes deployment** (modern infrastructure)
- **Observability** (analytics dashboard, Prometheus integration)
- **Self-service onboarding** (no manual work)
- **Professional support** (KB, support tickets, health monitoring)
**and charge them $X/month."**

You're now a real SaaS product, not a side project.

---

## Next: Phase 6 Planning

After Phase 5, you'll build:
- Billing/payment (Stripe integration)
- SLA monitoring (uptime guarantees)
- Advanced RBAC (custom roles)
- API rate limiting
- Audit compliance features
- Self-hosted option support

For now: **Focus on Phase 5. Ship it. Get customers.**
