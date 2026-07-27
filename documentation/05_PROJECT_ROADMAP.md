# 05_PROJECT_ROADMAP

## Current Phase
Phase 1: validate the complete investigation workflow using a mock agent and a controlled demo Payment API.

## Completed Foundation
- Repository and architectural documentation
- ASP.NET Core and Angular foundations
- MongoDB and authentication foundations
- Shared libraries and CI configuration
- Applications vertical slice
- Initial Investigation and RuntimeProbe domain models

These foundations are implementation progress, not proof of product-market fit or runtime feasibility.

## Phase 1 - Workflow Validation
Build the complete product loop without runtime instrumentation:

1. Create and manage an investigation in the UI.
2. Select a controlled demo application with intentional production-like defects.
3. Create a temporary probe with target, capture policy, limits, and expiry.
4. Persist and audit the request.
5. Dispatch it through the versioned protocol.
6. Activate it in a mock agent.
7. Return and display structured mock evidence in the UI.
8. Remove it manually and automatically at expiry.

### Phase 1 Exit Criteria
- Design partners complete the workflow without assistance.
- Probe states and failures are visible and auditable.
- Disconnect, retry, idempotency, revocation, and expiration work end to end.
- Customer interviews confirm the generated evidence would change an investigation outcome.
- The mock agent passes the shared agent contract suite.
- The demo Payment API can reproduce the intended investigation scenarios reliably.
- The UI is strong enough to demo to customers without explaining away missing pieces.

## Phase 2 - Minimum .NET Runtime Agent
Build one production agent with a deliberately narrow scope:

- Safe installation, registration, authentication, and capability reporting
- Dynamic log probes
- Method entry and exit capture
- Policy-controlled parameter capture
- Exception capture
- Manual removal, local expiry, kill switch, and overhead telemetry

Before implementation, run a technical spike comparing supported .NET instrumentation mechanisms. Arbitrary method instrumentation without source changes will likely require CLR Profiling API/ReJIT. Validate attachment, optimized code, async methods, symbols, containers, single-file deployment, trimming, and runtime-version compatibility early.

### Phase 2 Exit Criteria
- Demonstrated against representative optimized .NET services
- Measured overhead remains within a documented budget
- No probe survives expiry, revocation, disconnect policy, or process restart
- Sensitive values are blocked or redacted by policy
- Agent installation and rollback are repeatable

## Phase 3 - Safer and Richer Probes
- Conditional probes
- Return values
- Local variables where feasible
- Timing
- Rate limiting, backpressure, and aggregation
- Automatic expiration hardening
- Evidence export to one existing observability backend

## Phase 4 - Runtime Expansion
Add another runtime only after .NET has repeatable adoption and acceptable economics:

1. Java
2. Python
3. Node.js

Each runtime uses its own instrumentation implementation but shares probe lifecycle, protocol semantics, safety policy, evidence schema, and audit behavior.

## Later Opportunities
- Assisted hypothesis generation
- Cross-provider evidence correlation
- IDE and incident-management integrations
- Audit timeline analytics
- Carefully bounded automated investigations

## Known Risks
- Runtime attachment differs across versions, deployment models, optimization modes, and security policies.
- Parameters, return values, and locals can expose sensitive information.
- Dynamic instrumentation can affect latency or stability if limits fail.
- Telemetry volume can grow unexpectedly.
- Provider behavior varies across vendors.
- Supporting several runtimes too early can fragment the team and erase the technical moat.
- A mock workflow can validate usability but cannot prove safe runtime instrumentation.
- A weak demo application will hide product value, so the controlled Payment API must behave like a convincing production environment.

## Required Foundation Hardening
- Complete health checks and database initialization
- Replace development credentials and hard-coded frontend configuration
- Complete user persistence and role enforcement
- Finish missing Angular shell pages and responsive behavior
- Define evidence retention, encryption, and deletion policies
- Add protocol compatibility and end-to-end lifecycle tests
