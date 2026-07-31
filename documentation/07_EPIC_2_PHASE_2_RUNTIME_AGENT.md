# Epic 2 - Phase 2 Runtime Agent

## Objective
Build the minimum production .NET runtime agent and the supporting control-plane pieces needed to replace the mock agent with a real, safe, and testable runtime workflow.

## Ticket 11 - Control-Plane Health and Startup Hardening
Goal

Make the platform start predictably in local development and CI.

Deliverables

- Backend health checks
- Demo workflow startup validation
- Docker Compose readiness notes
- Safer API configuration defaults

Acceptance Criteria

- API exposes a healthy readiness endpoint
- Startup failures are visible and fail fast
- Local demo services have consistent startup behavior

## Ticket 12 - Investigation Module API
Goal

Turn investigations into a proper first-class API module.

Deliverables

- Investigation create/list/detail endpoints
- Investigation status model
- UI access to investigations
- Workflow linking for probe creation

Acceptance Criteria

- Investigations can be created and viewed through the UI
- A probe can be launched from an investigation context

## Ticket 13 - Runtime Probe Control Contracts
Goal

Define the versioned contracts between the control plane and agents.

Deliverables

- Agent registration contract
- Probe activation contract
- Evidence contract
- Probe removal contract
- Protocol versioning

Acceptance Criteria

- Contracts are serializable and testable
- Mock agent and future .NET agent can share the same contract tests

## Ticket 14 - Dispatcher Service
Goal

Introduce a dispatcher abstraction that owns command delivery to agents.

Deliverables

- Dispatcher service interface
- Mock agent client
- Activation and removal command flow
- Evidence polling or push hook

Acceptance Criteria

- Dispatcher can activate and remove a probe
- Evidence can be ingested from the mock agent

## Ticket 15 - Demo Workflow Persistence
Goal

Persist the demo workflow state instead of keeping it in memory.

Deliverables

- Mongo-backed persistence for investigations, probes, evidence, and audit entries
- Repository abstraction
- Basic indexes
- Seed/reset support for local demo usage

Acceptance Criteria

- Demo workflow survives API restarts when MongoDB is present
- UI still reads and writes the same workflow model

## Ticket 16 - Mock Agent Evidence Loop
Goal

Make the mock agent emit evidence based on deployed probes.

Deliverables

- Probe activation endpoint integration
- Simulated evidence generation
- Removal and expiry handling
- Evidence status updates

Acceptance Criteria

- Deploying a probe can create visible evidence in the platform

## Ticket 17 - Investigation Timeline
Goal

Show the complete investigation lifecycle in the UI.

Deliverables

- Timeline component
- Probe and evidence markers
- Audit event mapping

Acceptance Criteria

- Users can see the workflow progress from investigation to evidence

## Ticket 18 - Runtime Agent Skeleton
Goal

Create the first production agent project.

Deliverables

- Agent project structure
- Registration bootstrap
- Configuration loading
- Local diagnostics

Acceptance Criteria

- Agent project builds and starts

## Ticket 19 - Agent Capability Reporting
Goal

Let the agent report what it can safely do.

Deliverables

- Capability model
- Registration response
- UI display of agent capabilities

Acceptance Criteria

- Control plane can distinguish supported from unsupported probe requests

## Ticket 20 - Local Probe Safety Policy
Goal

Enforce a local agent-side safety policy.

Deliverables

- Capture allow/deny rules
- Payload and rate limits
- Expiry enforcement

Acceptance Criteria

- Agent can reject unsafe commands even if the control plane approved them

## Ticket 21 - Instrumentation Spike Harness
Goal

Build a small spike harness for CLR instrumentation exploration.

Deliverables

- Representative sample app
- Attachment experiments
- Logging for runtime behavior

Acceptance Criteria

- We can compare instrumentation approaches against a stable test harness

## Ticket 22 - Optimized Build Compatibility Tests
Goal

Prove agent behavior against release-optimized builds.

Deliverables

- Release-mode compatibility tests
- Async/multi-thread behavior coverage
- Symbol and trimming checks

Acceptance Criteria

- Agent-related tests run against optimized builds

## Ticket 23 - Expiry and Removal Guarantees
Goal

Prove that probes cannot outlive their authorization.

Deliverables

- Expiry enforcement tests
- Removal tests
- Disconnect/retry safety tests

Acceptance Criteria

- A probe cannot remain active after expiry or removal

## Ticket 24 - Evidence Redaction Rules
Goal

Protect sensitive data before evidence leaves the agent.

Deliverables

- Redaction policy
- Secret detection rules
- Server-side validation

Acceptance Criteria

- Sensitive values are blocked or redacted by default

## Ticket 25 - Audit Trail Completion
Goal

Make the audit trail complete and queryable.

Deliverables

- Audit event schema
- Audit ingestion
- Audit UI refinements

Acceptance Criteria

- Every meaningful probe lifecycle action is visible in the audit trail

## Ticket 26 - Agent Health and Telemetry
Goal

Add operational visibility to the agent.

Deliverables

- Health endpoint
- Diagnostics logs
- Basic metrics hooks

Acceptance Criteria

- The operator can see whether the agent is healthy and connected

## Ticket 27 - Demo Workflow Cleanup
Goal

Remove rough edges from the demo experience.

Deliverables

- One-click probe launch from investigation cards
- Default sample data
- Better empty states

Acceptance Criteria

- The demo can be walked without manual IDs or hidden setup

## Ticket 28 - Backend Contract Tests
Goal

Lock the control-plane APIs with contract tests.

Deliverables

- Request/response shape tests
- Protocol compatibility tests
- Failure-path coverage

Acceptance Criteria

- Breaking API changes are caught before merge

## Ticket 29 - Agent Command Retry Behavior
Goal

Make agent delivery resilient to temporary failures.

Deliverables

- Retry policy
- Idempotency checks
- Duplicate command protection

Acceptance Criteria

- Commands can be retried safely

## Ticket 30 - Phase 2 Demo Readiness
Goal

Package the Phase 2 milestone into something that can be demoed and tested.

Deliverables

- Final walkthrough notes
- Demo scripts
- Known limitations list

Acceptance Criteria

- The team can test and demo the workflow end to end with clear setup steps
