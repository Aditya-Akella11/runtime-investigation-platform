# 03_TECH_STACK_AND_STANDARDS

## Frontend
- Angular
- Angular Material
- RxJS

## Control Plane
- ASP.NET Core
- Clean Architecture inside a modular monolith
- REST/HTTPS for public APIs
- MongoDB
- Docker

## Runtime Agent
- .NET is the first supported production runtime
- A mock agent precedes production instrumentation and powers the first demoable MVP
- Versioned, transport-independent command and evidence contracts
- HTTPS first; WebSocket or gRPC only when measurements justify streaming
- CLR Profiling API and ReJIT are likely mechanisms for arbitrary no-source-change .NET instrumentation; the final mechanism requires a focused technical spike
- Java, Python, and Node are explicitly deferred

## MVP Demo Environment
- A dedicated demo Payment API is part of the first milestone
- The demo service contains intentional failure modes for investigation scenarios
- The mock agent must be able to simulate activation, evidence, expiry, and removal against the demo service
- The same UI and probe contracts must later work against the real .NET agent

## Initial .NET Probe Scope
- Dynamic log probes
- Method entry and exit
- Selected parameters under capture policy
- Exceptions

## Deferred Probe Scope
- Conditional probes
- Return values
- Local variables where runtime metadata and optimization permit
- Timing
- Advanced rate limiting and evidence aggregation
- Additional language runtimes

## Authentication and Security
- JWT for users during the initial control-plane phase
- Roles: Admin, Investigator, Viewer
- Short-lived, scoped credentials for agents
- Mutual TLS or an equivalent mutually authenticated production transport
- Encryption at rest and in transit
- Local and server-side policy enforcement

## Testing
- xUnit
- Jasmine
- Contract tests shared by mock and production agents
- End-to-end probe lifecycle tests
- Agent overhead, expiration, disconnect, retry, and failure-injection tests
- Runtime-version and optimized-build compatibility tests

## Coding Standards
- Follow SOLID principles
- Use dependency injection
- Prefer repositories at persistence boundaries
- Keep logging structured and consistent
- Treat agent commands and captured values as untrusted input
- Make command handlers idempotent
- Require explicit lifetime, capture, payload, and rate bounds for every probe
- Keep runtime-specific code outside control-plane modules
- Do not advertise a probe capability until a contract test proves it

## Branching and Delivery
- Use short-lived feature branches
- Merge through pull requests
- Keep main deployable
- CI restores, builds, tests, analyzes, and checks formatting
- Agent releases are independently versioned and backward-compatible within a documented protocol window
