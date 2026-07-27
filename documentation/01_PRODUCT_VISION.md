# 01_PRODUCT_VISION

## Product Vision
The Runtime Investigation Platform helps engineering teams move from production symptoms to root cause using targeted, temporary, and auditable runtime evidence.

## Mission
Give engineers a safe workflow for requesting evidence that existing telemetry does not contain, without changing application source code or deploying a new application version.

## Problem Statement
Modern applications fail across distributed environments with fragmented signals. Logs, metrics, and traces often show that a failure occurred but not the runtime values or decisions that caused it. Engineers then reproduce issues, add permanent logging, redeploy, and wait for the problem to recur.

## Product Position
This is an investigation product, not a replacement for monitoring or observability platforms. It creates missing runtime evidence on demand and can send that evidence to systems customers already use.

## Differentiation and Technical Moat
The technical moat is the runtime agent and its safety system, not Angular, MongoDB, or the API framework. A defensible agent must:

- attach to approved .NET processes safely
- apply narrowly scoped temporary instrumentation with bounded overhead
- capture only policy-approved data
- enforce rate, size, lifetime, and targeting limits locally
- remove probes explicitly and automatically
- remain safe during control-plane disconnection
- export evidence to multiple observability backends
- make every operation traceable and auditable

The investigation workflow is the initial product hypothesis. The runtime agent becomes the moat only after that workflow is validated.

## Initial Market Wedge
- Runtime: .NET first
- User: production engineers investigating issues that existing telemetry cannot explain
- Workflow: incident symptom to targeted probe to runtime evidence to confirmed finding
- Integration posture: complement Datadog, Dynatrace, OpenTelemetry, and similar systems

## Customer Personas
- Engineering teams handling production incidents
- Site reliability engineers
- Platform and observability engineers
- Support engineers escalating complex customer issues

## Core Product Principles
- Evidence over assumptions
- Structured investigation over ad hoc notes
- Safe-by-default temporary instrumentation
- Automatic cleanup and bounded overhead
- Provider-neutral evidence delivery
- Traceable workflows and immutable auditability
- Human approval before increasingly powerful automation
- Start with one runtime and earn broader coverage

## Phase 1 MVP Goals
The MVP should prove the full customer experience, not runtime injection itself.

- Provide a demo environment that the team fully controls
- Let an engineer create, deploy, observe, and remove a runtime probe end to end
- Use a controlled demo application with intentional bugs and realistic production failure modes
- Simulate agent activation and evidence collection with a mock runtime agent
- Keep the UI live and customer-facing so the workflow can be demoed before the real agent exists
- Establish a versioned runtime-agent protocol before building runtime instrumentation

## Product Boundaries
- The platform does not promise zero operational setup; an authorized runtime agent must be installed.
- Autonomous remediation is not part of the initial product.
- Java, Python, and Node support follows validation of the .NET agent and workflow.
- A requested probe is not guaranteed to be feasible; the agent reports its capabilities explicitly.
- The demo Payment API is part of the MVP testing architecture and exists to prove the workflow before production agent rollout.

## Future Vision
Expand from a trusted .NET runtime investigation agent into additional runtimes, assisted investigation, and cross-provider evidence correlation. Automation remains explainable, auditable, bounded, and reversible.
