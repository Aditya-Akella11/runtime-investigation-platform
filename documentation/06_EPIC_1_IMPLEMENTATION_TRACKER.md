# Epic 1 Implementation Tracker

## Status Overview
- Ticket 1: Completed
- Ticket 2: Completed
- Ticket 3: Completed
- Ticket 4: Completed
- Ticket 5: Completed
- Ticket 6: Completed
- Ticket 7: Completed - shared results, errors, exceptions, base entity, validation, logging abstraction, constants, extensions, and exception middleware
- Ticket 8: Completed - GitHub Actions restore, build, backend/frontend test, static analysis, and formatting pipeline
- Ticket 9: Completed - pure Investigation and RuntimeProbe domain models, lifecycle rules, enums, and domain tests
- Ticket 10: Completed - Applications create, list, edit, and delete vertical slice across MongoDB, API, service, UI, and tests

## Progress Notes
- Repository structure, documentation foundation, and AI guidance files have been created.
- A .NET solution skeleton was initialized under backend/ with solution projects for API, application, domain, infrastructure, shared, and test projects.
- An Angular application was scaffolded under frontend/ with routing and a simple applications shell.
- The first vertical slice for applications is implemented on the backend and frontend.
- Shared foundations, automated CI, runtime probe domain rules, and the full Applications vertical slice are implemented.
- The post-Epic architecture is aligned around a modular control plane, a versioned agent protocol, a Phase 1 mock agent, and a .NET-first production agent.

## Next Delivery Phase
- Demo Payment API with intentional failure scenarios and Docker Compose support
- Investigation CRUD and investigation UI
- Probe definition, safety limits, lifecycle persistence, and audit events
- Transport-independent agent contracts
- HTTPS-based dispatcher
- Mock agent with activation, evidence, failure, disconnect, removal, and expiry scenarios
- End-to-end UI-to-agent workflow tests

Production CLR instrumentation, additional runtime languages, and advanced probe capture are explicitly outside Epic 1 and Phase 1.
The MVP is demo-first: prove the workflow against a controlled payment service before building the real .NET agent.

## Implemented Deliverables
- Initial README, LICENSE, .gitignore, and .editorconfig
- Architectural documentation set in documentation/
- .github/copilot-instructions.md for AI guardrails
- Backend solution and initial domain/application/infrastructure structure
- Angular app shell and applications route
- Basic application domain model and in-memory repository
- Initial backend and frontend tests
- Reusable shared result, validation, error handling, logging, constants, and middleware primitives
- GitHub Actions CI for backend and frontend quality gates
- Pure runtime probe and investigation domain lifecycle with unit tests
- Complete Applications CRUD in the API and Angular UI

## Implementation Rules
- Keep work aligned with the architecture documents in documentation/.
- Implement one ticket at a time.
- Preserve the existing folder structure and technology choices.
