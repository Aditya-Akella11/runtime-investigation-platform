# Phase 4 Implementation Plan

## Objective

Phase 4 turns the validated Phase 3 probe workflow into a repeatable runtime-agent delivery path. It covers the .NET 8 agent foundation, process attachment and instrumentation spikes, backend dispatch, frontend integration, evidence collection, cleanup, operational hardening, and release verification.

The ticket list below is the implementation backlog supplied for this phase. Templates are directional: implementations must use the namespaces, contracts, safety policies, and folder structure already established in this repository.

## Non-Negotiable Constraints

- The architectural documents in `documentation/` are the source of truth. Do not modify or reinterpret them as part of ticket work.
- Do not treat reflection over the agent's own `AppDomain` as cross-process instrumentation. Cross-process discovery and mutation require a validated CLR Profiling API/ReJIT or equivalent supported mechanism.
- Do not ship arbitrary native memory patching, unsafe function-pointer jumps, or undocumented runtime manipulation as production behavior. Keep such work behind an explicitly approved technical spike and a kill switch.
- Probe activation must remain authenticated, authorized, bounded, auditable, expiring, revocable, and fail closed.
- Evidence must use the shared protocol and schema, be redacted and size-limited at the agent boundary, and never contain secrets by default.
- Preserve existing API contracts, MongoDB collections, module names, and folder structure. Adapt ticket examples to the current codebase instead of copying incompatible templates.
- Every implementation ticket needs focused automated tests or a documented manual test where automation cannot safely prove the behavior.

## Delivery Shape

Six sprints are planned across approximately six weeks:

| Sprint | Focus | Tickets |
|---|---|---|
| 1 | Agent foundation | 1-8 |
| 2 | Attach and inject spike | 9-18 |
| 3 | Backend API connection | 19-30 |
| 4 | Frontend integration | 31-37 |
| 5 | Observability and cleanup | 38-46 |
| 6 | Testing and compatibility | 47-55 |

Ticket 17 and tickets 28-29 are intentionally unassigned in the supplied roadmap. Do not invent scope for them without an approved issue.

## Critical Dependencies

Follow these dependency gates:

- Ticket 1 precedes all other tickets.
- Ticket 2 precedes tickets 9 and 12.
- Ticket 3 precedes tickets 4 and 12.
- Ticket 4 and ticket 5 are prerequisites for the manual instrumentation test in ticket 15.
- Ticket 9 precedes tickets 11 and 12.
- Ticket 12 precedes tickets 14, 15, and 19.
- Ticket 19 precedes tickets 23 and 24.
- Ticket 20 and ticket 22 precede ticket 23.
- Ticket 24 and ticket 25 precede ticket 27.
- Ticket 25 and ticket 27 precede ticket 30.
- Ticket 31 precedes tickets 32 and 33.
- Ticket 33 precedes tickets 34 and 36.
- Ticket 36 and ticket 30 are prerequisites for ticket 37.
- Ticket 38 precedes ticket 39; ticket 40 precedes tickets 41-44; ticket 44 precedes ticket 45.
- Ticket 47 precedes tickets 48-49; ticket 49 precedes ticket 55.
- Ticket 51 precedes ticket 52; ticket 52 precedes ticket 53.

If a dependency is not green, skip the dependent ticket and record the blocker.

## Ticket Backlog

### Sprint 1 - Agent Foundation

#### Ticket 1 - Create RuntimeAgent.ConsoleApp

**Estimate:** 5 points. Create a .NET 8 console application, not a library. It must compile, accept `--pid`, and log `Agent started, PID: {pid}`. Use the existing RuntimeAgent project conventions rather than creating a parallel module.

#### Ticket 2 - Implement ProcessAttacher

**Estimate:** 8 points. Add Windows process-handle attachment with graceful handling of missing PIDs, access denial, and unsupported platforms. Detect the target runtime through an explicit inspection strategy and retain handles only for the owning lifecycle. Log successful attachment as `Successfully attached to PID {pid}`.

#### Ticket 3 - Create ProbeInjector method finder

**Estimate:** 8 points. Implement a safe method-filter parser for `Namespace.Class.Method`. Return a `MethodInfo` only for methods visible through the supported discovery boundary, handle reflection load failures, and return null for invalid or missing methods. Include a `System.Console.WriteLine` test fixture where appropriate.

#### Ticket 4 - Implement an IL wrapper spike

**Estimate:** 13 points. Use `DynamicMethod` only for an in-process, clearly labelled spike that logs method entry and preserves parameters and return type. Cover instance/static methods, argument forwarding, return values, and void methods. Do not present this wrapper as cross-process injection.

#### Ticket 5 - Create a SimpleApp sample

**Estimate:** 3 points. Add a .NET 8 target app that prints its PID and repeatedly calls `Calculator.Add` and `Multiply` for at least 100 seconds. Keep the app deterministic and suitable for manual instrumentation testing.

#### Ticket 6 - ProcessAttacher tests

**Estimate:** 3 points. Test attaching to the current process and graceful failure for an invalid PID. Platform-specific tests must be skipped or isolated when Windows process APIs are unavailable.

#### Ticket 7 - ProbeInjector tests

**Estimate:** 3 points. Test finding `System.Console.WriteLine` and returning null for an invalid method path. Add overload-selection coverage if the filter contract requires it.

#### Ticket 8 - IL wrapper tests

**Estimate:** 5 points. Test execution, correct return values, signature preservation, and entry logging for representative methods such as `string.ToUpper` and `int.Parse`.

### Sprint 2 - Attach and Instrumentation Spike

#### Ticket 9 - Implement the CLR profiler callback contract

**Estimate:** 13 points. Implement only the supported `ICorProfilerCallback` surface required by the approved profiling spike, including initialization and safe shutdown. Use valid COM metadata and HRESULT handling. Add a no-crash initialization test or manual harness; do not claim arbitrary instrumentation support until the runtime matrix is verified.

#### Ticket 10 - Register profiler environment

**Estimate:** 5 points. Centralize environment setup for `CORECLR_PROFILER`, `CORECLR_PROFILER_PATH`, `COR_PROFILER`, and `COR_PROFILER_PATH`, with explicit runtime selection and process-scoped configuration. Never place profiler identifiers or paths in hard-coded deployment assumptions.

#### Ticket 11 - Evaluate method hooking feasibility

**Estimate:** 13 points. Produce an approved technical spike for JIT/ReJIT or another supported mechanism. Validate architecture, executable-memory requirements, instruction-cache behavior, rollback, x86/x64 differences, optimized methods, async methods, and crash behavior. Raw jumps into target process memory are not production acceptance criteria without explicit architecture approval.

#### Ticket 12 - Create ProbeActivator

**Estimate:** 8 points. Orchestrate validated attach, method resolution, activation, and status logging. Return structured failure reasons, enforce authorization and policy checks, and never mark a probe active unless the instrumentation mechanism confirms activation.

#### Ticket 13 - Implement bounded LogBuffer

**Estimate:** 5 points. Store timestamped method evidence in a thread-safe bounded buffer. Use a `ConcurrentQueue` or equivalent with explicit maximum size, dropped-entry accounting, redaction, and non-blocking producer behavior.

#### Ticket 14 - Wire the agent lifecycle

**Estimate:** 3 points. Support command parsing for activate, deactivate, status, and shutdown through the existing agent protocol. Respond with versioned structured results rather than relying on fragile free-form stdin parsing.

#### Ticket 15 - Manual SimpleApp integration test

**Estimate:** 5 points. Run SimpleApp, connect the agent through the supported path, activate `SimpleApp.Calculator.Add`, verify at least five bounded evidence entries, and confirm both target and agent remain healthy. Record unsupported scenarios instead of treating a reflection wrapper as proof of cross-process support.

#### Ticket 16 - Detach and cleanup

**Estimate:** 5 points. Remove active probes, restore state through the supported instrumentation API, flush bounded evidence, release handles and callbacks, and exit cleanly. Cleanup must be idempotent and also run on expiry, cancellation, and process exit.

#### Ticket 18 - Measure overhead

**Estimate:** 8 points. Add a repeatable benchmark comparing the target with and without a probe. Report measured latency, allocation, dropped evidence, and environment details. The 5% value is a budget to measure and investigate, not a guaranteed acceptance threshold for every runtime.

### Sprint 3 - Backend API Connection

#### Ticket 19 - Add observability provider

**Estimate:** 8 points. Implement the existing provider abstraction for Datadog or a mock sink. Export timestamp, method, arguments subject to policy, and probe identity through a bounded asynchronous queue. The provider is configurable and disabled by default.

#### Ticket 20 - Implement HTTP probe dispatch

**Estimate:** 8 points. Implement `IProbeDispatcher` using the versioned agent contract, cancellation, timeout, authentication, response validation, and safe network failure handling. Avoid creating an unconfigured `HttpClient` per request.

#### Ticket 21 - Implement CreateInvestigation

**Estimate:** 5 points. Create a Draft investigation with a unique ID, UTC creation time, validated application identity, and the repository's existing aggregate shape. Persist and return the created entity.

#### Ticket 22 - Implement AddProbe

**Estimate:** 5 points. Validate the investigation, create a probe with a policy-approved default expiry, initialize its lifecycle status, and persist it. Reject malformed filters and unsafe capture policy before storage.

#### Ticket 23 - Implement ActivateProbe

**Estimate:** 8 points. Validate probe state and policy, dispatch through the agent contract, and update status and activation time only after success. Preserve failure state and audit context when dispatch fails.

#### Ticket 24 - Implement investigation endpoints

**Estimate:** 8 points. Add the existing API's create-investigation, add-probe, and activate-probe endpoints with correct status codes, validation responses, authorization, and contract tests. Include the required investigation retrieval route used by `CreatedAtAction`.

#### Ticket 25 - Add the agent HTTP command endpoint

**Estimate:** 5 points. Expose the authenticated, versioned probe activation endpoint in the RuntimeAgent host using the repository's supported hosting model. Include health/status behavior, request validation, and graceful shutdown.

#### Ticket 26 - Implement Mongo investigation persistence

**Estimate:** 5 points. Implement the existing `IInvestigationRepository` contract for insert and lookup using the established Mongo collection and serialization conventions. Return null when absent and honor cancellation tokens.

#### Ticket 27 - API-to-agent integration test

**Estimate:** 8 points. Cover create investigation, add probe, activate probe, agent dispatch, and response handling with a controlled fake agent. Do not depend on a real PID or external MongoDB in the default test suite.

#### Ticket 30 - Local container stack

**Estimate:** 5 points. Make the API, agent, target app, frontend, and MongoDB compose configuration start with documented health checks, configuration, ports, and dependency readiness. The stack must not expose development credentials or assume privileged runtime memory manipulation.

### Sprint 4 - Frontend Integration

#### Ticket 31 - Create investigation service

**Estimate:** 5 points. Add typed Angular API methods for listing and retrieving investigations, creating investigations, adding probes, and activating or deactivating probes. Use the existing API base-URL and authentication configuration.

#### Ticket 32 - Update investigations list

**Estimate:** 5 points. Load investigations on initialization, display lifecycle state and useful metadata, and provide a validated new-investigation form. Avoid browser `prompt` and `alert` flows in the production UI.

#### Ticket 33 - Create investigation detail view

**Estimate:** 8 points. Load an investigation by route ID, display its probes and errors, and provide accessible forms for adding and activating probes. Reflect asynchronous loading, failure, and pending states.

#### Ticket 34 - Add investigation routing

**Estimate:** 3 points. Add and test the `/investigations/:id` route using the existing Angular standalone routing conventions and route-level authorization behavior.

#### Ticket 35 - Update shell navigation

**Estimate:** 3 points. Add navigation for Dashboard, Investigations, Applications, and Audit using existing shell components and active-link styling. Routes must remain keyboard and screen-reader accessible.

#### Ticket 36 - Add probe-list display

**Estimate:** 3 points. Render method filter, status, creation and expiry information, and actions in a responsive accessible table or equivalent. Enable activation only for valid Draft probes and show pending/error states.

#### Ticket 37 - Frontend-to-agent manual E2E test

**Estimate:** 5 points. Run the local stack, create an investigation, add a probe, activate it, verify status transition, and inspect bounded evidence. Record the target runtime, agent version, and any unsupported instrumentation limitations.

### Sprint 5 - Observability and Cleanup

#### Ticket 38 - Probe expiration and auto-cleanup

**Estimate:** 8 points. Run a cancellation-aware background service that finds expired probes, dispatches deactivation, records failures, and marks probes Expired only after cleanup semantics are satisfied. Prevent duplicate cleanup.

#### Ticket 39 - Register expiration service

**Estimate:** 2 points. Register the hosted service with all required repository, dispatcher, logging, and cancellation dependencies. Add startup and shutdown coverage.

#### Ticket 40 - Add ProbeResult entity

**Estimate:** 5 points. Create the separate probe-result model with probe ID, timestamp, method, bounded arguments, optional return value, and execution duration using the shared evidence conventions.

#### Ticket 41 - Add IProbeResultRepository

**Estimate:** 5 points. Define add, query-by-probe, and retention cleanup operations with cancellation and pagination/limits where required by the API contract.

#### Ticket 42 - Implement Mongo probe-result repository

**Estimate:** 5 points. Persist, query, and delete results using a separate collection, established indexes, retention rules, and the project's Mongo configuration. Test missing and large-result cases.

#### Ticket 43 - Add probe-results endpoint

**Estimate:** 3 points. Add an authorized `GET /probes/{id}/results` endpoint returning a bounded result set with timestamp, method, and arguments. Do not allow arbitrary unbounded queries.

#### Ticket 44 - Upload results from the agent

**Estimate:** 8 points. Convert local entries to the shared result contract and upload through a bounded, retry-aware background path. Handle network errors without crashing or blocking target execution; apply authentication and redaction.

#### Ticket 45 - Add deactivation API

**Estimate:** 5 points. Add the authorized deactivation workflow, dispatch cleanup to the agent, and mark the probe Closed with a UTC close time only after successful cleanup. Make repeated deactivation idempotent.

#### Ticket 46 - Audit probe lifecycle actions

**Estimate:** 5 points. Record activation and deactivation actor, timestamp, probe/investigation identity, result, and safe structured details through the existing audit abstraction. Do not serialize identity or secrets into unescaped free-form fields.

### Sprint 6 - Compatibility, Quality, and Release

#### Ticket 47 - Create a .NET Framework sample

**Estimate:** 5 points. Add a separate legacy sample targeting .NET Framework 4.7.2 only if the supported build environment can compile and run it. Print its PID and exercise representative methods.

#### Ticket 48 - Adapt runtime environment selection

**Estimate:** 8 points. Detect supported .NET Framework versus .NET Core/.NET versions and set only the appropriate profiler variables. Keep .NET 8 behavior covered and fail clearly for unsupported runtimes.

#### Ticket 49 - Test legacy application support

**Estimate:** 8 points. Run the legacy sample through the approved profiler path, verify evidence and cleanup, and record platform prerequisites. A passing manual test cannot substitute for a supported runtime matrix.

#### Ticket 50 - Harden error handling

**Estimate:** 8 points. Cover invalid PIDs, missing processes, permissions, process exit, malformed commands, network timeouts, cancellation, and provider failures. Return clear safe errors and preserve auditability without leaking sensitive details.

#### Ticket 51 - Add comprehensive service tests

**Estimate:** 5 points. Test investigation, probe, expiration, dispatch, result, authorization, validation, and edge-case behavior. Measure meaningful service coverage; do not add skipped tests to make the suite appear complete.

#### Ticket 52 - Configure CI

**Estimate:** 5 points. GitHub Actions must restore, build, test, and publish coverage for the supported solution and frontend. Pin supported tool versions and fail on test or coverage-reporting errors.

#### Ticket 53 - Stage the compose deployment

**Estimate:** 5 points. Deploy the documented stack to a clean staging VM, verify health endpoints and frontend loading, create an investigation, and exercise activation against the controlled target. Capture configuration and rollback notes.

#### Ticket 54 - Write the deployment guide

**Estimate:** 8 points. Document prerequisites, installation, configuration, authentication, supported runtimes, safe operation, cleanup, troubleshooting, and examples. Include screenshots or reproducible UI examples where the workflow requires them; never document real secrets.

#### Ticket 55 - Add performance and stress tests

**Estimate:** 8 points. Exercise at least 1,000 concurrent tasks and 100,000 method calls where the target sample supports it. Measure latency, allocations, queue pressure, dropped evidence, and cleanup. Treat overhead and memory targets as measured budgets with environment details.

## Working Instructions

### Daily Workflow

1. Pick the next unfinished ticket from the active sprint.
2. Read its acceptance criteria and inspect the owning abstraction and neighboring tests.
3. State the local behavior hypothesis and the cheapest check that can disprove it.
4. Implement the smallest repository-consistent change.
5. Run the focused test, build, or manual check immediately.
6. Verify every acceptance criterion and update the ticket status.
7. Record blockers and move to the next unblocked ticket.
8. Use commit messages in the form `Ticket #XX: <title>` when commits are requested by the project workflow.

### Sprint Sanity Checks

```text
Sprint 1: dotnet test backend/RuntimeInvestigation.sln --filter "ProcessAttacher|ProbeInjector|ILInjector"
Sprint 2: dotnet build backend/RuntimeInvestigation.sln; run the approved SimpleApp instrumentation harness
Sprint 3: dotnet test backend/RuntimeInvestigation.sln --filter "API|E2E|Protocol"
Sprint 4: npm run build --prefix frontend; npm test --prefix frontend -- --watch=false
Sprint 5-6: dotnet test backend/RuntimeInvestigation.sln; npm run build --prefix frontend; docker compose config
```

Commands must be adapted to actual project paths and test framework names. A manual test is successful only when its evidence is recorded.

### Quality Gate

Before marking a ticket complete, confirm:

- The relevant project compiles without new warnings.
- Focused tests pass, including failure and cancellation paths.
- Configuration is injected rather than hard-coded.
- No secrets, unbounded queues, unbounded evidence, or unsafe default capture are introduced.
- Logging is structured, useful, and redacted.
- Authorization, audit, expiry, revocation, and cleanup behavior are covered where relevant.
- The change preserves existing contracts and is documented when it changes operator behavior.

### Blocker Handling

- Attachment failure: verify PID, permissions, OS support, runtime support, and profiler registration before changing instrumentation code.
- Instrumentation instability: disable the probe, collect a crash-safe diagnostic, and revert to the supported profiling path; do not widen native patching.
- MongoDB failure: verify compose health and configuration, then use a test double for unit tests.
- Agent command failure: inspect protocol version, authentication, timeout, and agent health before retrying.
- Missing evidence: verify method-filter syntax, policy redaction, sampling/rate limits, queue drops, and target runtime compatibility.
- Performance regression: capture a baseline, inspect queue and serialization costs, and reduce capture scope before increasing limits.

## Phase Exit Criteria

Phase 4 is complete only when the supported .NET path has a repeatable create, authorize, dispatch, activate, collect, expire, deactivate, audit, and delete/retain workflow; the API and frontend checks pass; the local stack starts from a clean checkout; the instrumentation mechanism's supported runtime matrix is documented; and measured performance, safety, and failure behavior are accepted by the project owner.
