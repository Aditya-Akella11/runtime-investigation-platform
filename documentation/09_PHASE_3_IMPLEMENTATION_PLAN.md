# Phase 3 Implementation Plan

## Objective

Phase 3 evolves the Runtime Investigation Platform from a validated runtime-agent prototype into an enterprise SaaS product with richer probes, a real-time investigation workspace, operational safeguards, and integration-ready evidence.

Work is split into two batches. A ticket is complete only when its implementation, automated tests, documentation, and demo path are all verified.

## Batch 1: Rich Probes and Interactive Investigation UI

| Ticket | Deliverable | Acceptance criteria | Status |
|---|---|---|---|
| 31 | Code viewer and probe pinning | Source view exposes line gutters; selecting a line opens a probe form with target class, method, and line; deployment is visible on that line. | In progress |
| 32 | Live evidence stream | Authenticated SSE endpoint emits evidence and keep-alives; UI reconnects automatically and supports pause/resume without losing the existing feed. | In progress |
| 33 | Conditional expression engine | Agent evaluates a bounded comparison grammar without executing code; invalid expressions fail closed; numeric and string comparisons are tested. | In progress |
| 34 | Return values and call duration | Method-exit evidence supports bounded return values and monotonic duration measurements with explicit units. | In progress |
| 35 | Bounded local-variable capture | Collection length, object depth, string length, and sensitive-value redaction are enforced before evidence leaves the agent. | In progress |
| 36 | Stack trace and variable inspector | UI renders expandable, accessible trees for structured variables and exception details. | Not started |
| 37 | Rate limiting and backpressure | Per-probe token bucket and bounded ring buffer prevent unbounded memory or telemetry volume; dropped-event metrics are exposed. | In progress |
| 38 | Evidence search and filters | Evidence can be filtered by correlation ID, probe ID, application, target class, and payload text. | In progress |
| 39 | Hypotheses and findings workspace | Investigations support hypotheses with lifecycle status and links to supporting or contradicting evidence. | In progress |
| 40 | Batch 1 verification | Backend unit/contract/integration tests and Angular component/build checks cover tickets 31-39. | Not started |

## Batch 2: Enterprise Operations and Demo Readiness

| Ticket | Deliverable | Acceptance criteria | Status |
|---|---|---|---|
| 41 | OpenTelemetry and Datadog export | Background exporter is bounded, retryable, configurable, observable, and disabled by default. | Not started |
| 42 | Incident webhooks | Signed, allow-listed webhook delivery supports Slack/PagerDuty adapters, retries, and audit records. | Not started |
| 43 | RBAC hardening | Admin, Investigator, and Auditor policies are enforced server-side and covered by authorization tests. | Not started |
| 44 | Audit query and SOC 2 export | Authorized users can export stable, escaped CSV or JSON audit data with filters and pagination. | Not started |
| 45 | Heartbeat kill switch | Agents deactivate probes after 60 seconds without control-plane contact and cannot reactivate them without a fresh command. | Not started |
| 46 | Payment chaos console | UI triggers bounded concurrent payments, timeout spikes, duplicate charges, and null-reference scenarios with clear reset controls. | In progress |
| 47 | Enterprise design system | Shared tokens and responsive, accessible components implement the slate/cyan/emerald dark theme. | In progress |
| 48 | End-to-end workflow tests | Automated test covers investigation creation through probe removal, including chaos injection and streamed evidence. | Not started |
| 49 | Performance harness | Repeatable benchmarks report condition evaluation and capture overhead; targets are treated as measured budgets, not assumed guarantees. | Not started |
| 50 | Release and pitch demo | Documentation, containers, migration/configuration notes, demo script, and release checklist are verified from a clean environment. | Not started |

## Delivery Order

1. Stabilize protocol and safety primitives: tickets 33-35 and 37.
2. Complete the real-time vertical slice: tickets 31, 32, 36, 38, and 39.
3. Close Batch 1 with ticket 40 and an end-to-end demo checkpoint.
4. Add enterprise controls before outbound integrations: tickets 43-45, then 41-42.
5. Complete demo polish, E2E coverage, benchmarks, and packaging: tickets 46-50.

## Current Work-In-Progress Inventory

The working tree already contains initial implementations for line-targeted probes, SSE evidence streaming, condition evaluation, enriched method-exit evidence, variable bounds, ring-buffer rate limiting, evidence filtering, hypotheses, the chaos console, and visual refinements. These are marked **In progress** until their acceptance criteria and integration tests are complete.

## Verification

- Backend: `dotnet test backend/RuntimeInvestigation.sln`
- Frontend build: `npm run build --prefix frontend`
- Frontend tests: `npm test --prefix frontend -- --watch=false`
- End-to-end: create investigation, pin a conditional probe, inject a bounded fault, observe streamed evidence, attach evidence to a finding, remove the probe, and confirm the audit trail.

## Safety and Architecture Notes

- Probe conditions use a small allow-listed grammar and fail closed on invalid input.
- Evidence is bounded and redacted at the agent boundary; the UI is not a security boundary.
- Streaming must not use request-scoped response writers from concurrent event callbacks without serialized writes and deterministic unsubscription.
- Exporters and webhook dispatchers run out of process-path hot loops through bounded background queues.
- RBAC is enforced by API authorization policies, not by hidden UI controls.
- Chaos endpoints are development/demo-only, authenticated where appropriate, rate limited, and disabled by default outside demo environments.
