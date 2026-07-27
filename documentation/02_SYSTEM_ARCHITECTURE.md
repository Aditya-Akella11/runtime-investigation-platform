# 02_SYSTEM_ARCHITECTURE

## High-Level Architecture

```text
                         Angular UI
                              |
                       Investigation API
                              |
          +-------------------+-------------------+
          |                   |                   |
 Investigation Engine  Runtime Probe Engine  Evidence Engine
          |                   |                   |
          +-------------------+-------------------+
                              |
                         Audit Engine
                              |
                       Probe Dispatcher
                              |
                    HTTPS / WebSocket / gRPC
                              |
                  Runtime Agent / Mock Agent
                   (.NET first; other runtimes later)
                              |
                    Runtime Instrumentation
                              |
                       Demo or Running Process
```

The API, engines, audit capability, and dispatcher begin as modules in one ASP.NET Core modular monolith. They are logical boundaries, not separate microservices. The runtime agent is a separately deployed data-plane component with a versioned protocol. For the MVP, the mock agent and demo Payment API are part of the controlled testing environment so the full workflow can be validated before real instrumentation.

## Control-Plane Components

### Angular UI
Provides investigation, probe, evidence, audit, application, and agent-management workflows.

### Investigation API
Is the authenticated entry point. It maps transport requests into application commands and queries but does not contain runtime-specific instrumentation logic.

### Investigation Engine
Owns investigation lifecycle, hypotheses, findings, and links between probes and evidence.

### Runtime Probe Engine
Validates probe definitions and governs targeting, authorization, state transitions, rate limits, activation, expiration, and removal.

### Evidence Engine
Accepts structured agent results, associates evidence with its probe and investigation, applies retention and redaction rules, and exports evidence through provider adapters.

### Audit Engine
Maintains append-only records for probe requests, approvals, dispatch, activation, evidence access, expiration, removal, and failures.

### Probe Dispatcher
Maintains authenticated agent sessions and delivers commands idempotently. The protocol supports registration, capabilities, heartbeats, acknowledgements, results, cancellation, expiration, and version negotiation.

### Database
MongoDB stores applications, investigations, probes, evidence metadata, agent metadata, users, and audit records. Large evidence payloads may move to object storage when measurements justify it.

### Provider Adapters
Adapters export evidence and enrich investigations using systems such as Datadog, Dynatrace, and OpenTelemetry. Providers do not own core probe lifecycle behavior.

## Data-Plane Components

### Mock Agent
The first agent implementation exercises the real protocol and lifecycle without modifying a process. It supports deterministic activation, evidence, failure, disconnect, retry, removal, and expiry scenarios. It can drive a controlled demo application so the UI, investigation workflow, and audit trail can be proven before the real agent exists.

### .NET Runtime Agent
The first production agent receives authorized commands, checks local policy and capabilities, applies temporary instrumentation, emits bounded evidence, and removes instrumentation on command or expiry.

### Future Runtime Agents
Java, Python, and Node agents may be added after .NET validation. They share protocol semantics, probe lifecycle, evidence contracts, and safety policies but use runtime-specific instrumentation implementations.

## Probe Lifecycle
1. A customer issue creates an investigation.
2. An investigator creates a targeted probe with limits and expiry.
3. The Runtime Probe Engine validates and authorizes it.
4. The dispatcher sends an idempotent command to a capable agent.
5. The agent applies local policy and acknowledges activation or rejection.
6. The agent returns bounded, structured evidence.
7. The Evidence Engine stores and optionally exports the result.
8. The probe is removed explicitly or automatically at expiry.
9. Every transition is appended to the audit record and shown in the UI.

## MVP Demo Loop
The initial customer-facing demo uses a controlled .NET Payment API with intentional defects such as duplicate charges, intermittent failures, slow dependencies, timeout exceptions, and retry bugs. The mock agent simulates probe activation and captures evidence so the platform can demonstrate:

- investigation creation
- application selection
- probe definition
- probe deployment
- live results streaming
- probe removal and expiry

The same UI and control-plane workflow later drive the real .NET agent without changing the product surface.

## Agent Protocol
- Domain contracts remain independent of transport.
- HTTPS request/response and polling are the Phase 1 default.
- WebSocket or gRPC streaming is introduced only when measured latency or bidirectional communication requirements justify it.
- Commands include a unique identifier, protocol version, target, capability requirement, capture policy, rate and payload limits, and absolute expiry.
- Commands and results are idempotent and safe to retry.
- Capability negotiation prevents unsupported probes from being dispatched.

## Dependency Rules
- Frontend depends on backend APIs.
- Control-plane modules do not depend on runtime-specific instrumentation code.
- Provider adapters remain behind application interfaces.
- Runtime agents depend on shared protocol contracts, not control-plane persistence.
- The mock and production agents pass the same contract suite.
- Both server and agent enforce expiration and removal.

## Security and Safety Principles
- Authentication and role authorization are required for operational workflows.
- Production transport uses short-lived agent credentials and mutual authentication.
- Audit records are append-only and traceable.
- Evidence is encrypted in transit and at rest.
- Probe target, condition, fields, rate, payload size, and lifetime are bounded.
- Capture of secrets or personal data is denied or redacted by default.
- The agent has a local kill switch and the server supports revocation.
- Loss of control-plane connectivity cannot leave a probe active past local expiry.
- Probe capability and policy checks occur in the agent even when the server approved the request.

## Reliability Requirements
- Backpressure and quotas prevent evidence floods.
- Activation and removal acknowledgements are observable.
- Agent CPU, memory, latency, and payload overhead have documented budgets.
- Failed or disconnected agents never block the control plane.
- A stale command cannot reactivate an expired or removed probe.

## Deployment
The control plane is containerized. MongoDB and credentials use environment-specific configuration. Runtime agents are installed close to target processes and are independently versioned, upgraded, disabled, and rolled back.
