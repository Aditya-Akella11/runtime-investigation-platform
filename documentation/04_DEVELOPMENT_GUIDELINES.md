# 04_DEVELOPMENT_GUIDELINES

## Development Philosophy
Work incrementally, keep changes scoped, and preserve the existing architecture.

## Incremental Development
Implement one module or feature at a time and verify it before moving on.

## Definition of Done
- Code is implemented and tested
- Documentation is updated where needed
- The change aligns with the architectural documents

## Architecture Rules
- Do not redesign the system without approval
- Keep module boundaries clear
- Reuse existing patterns rather than introducing new ones
- Keep the control plane as a modular monolith until measured scaling or deployment needs justify extraction
- Keep runtime-specific instrumentation outside the control-plane projects
- Implement and test the mock agent protocol before production runtime instrumentation
- Do not add another language runtime until the .NET agent meets the roadmap exit criteria
- Treat the demo Payment API as part of the product surface for MVP validation, not as throwaway scaffolding
- Keep the customer-visible UI unchanged when replacing the mock agent with the real agent

## Coding Rules
- Write readable and maintainable code
- Keep changes small and focused
- Favor explicit configuration over hidden behavior

## Testing Rules
- Add or update tests for changed behavior
- Prefer integration-style tests for service boundaries
- Avoid fragile test-only code
- Test probe expiry and removal in both the server and agent
- Use shared contract tests for the mock agent and every production agent
- Measure runtime overhead against optimized representative applications
- Validate the full workflow in a controlled demo environment before investing in runtime instrumentation depth

## Runtime Safety Rules
- Every probe requires an absolute expiry
- Every capture has explicit rate, payload, and field limits
- Agent-side policy can reject any server-approved command
- Disconnects and retries must not duplicate probes or extend their lifetime
- Sensitive values are denied or redacted by default
- Runtime changes must be reversible and observable

## Documentation Rules
- Update documentation when behavior or workflow changes materially
- Keep architectural documents as the source of truth

## AI Collaboration Rules
- Avoid broad rewrites
- Ask before changing architecture
- Follow the project conventions established in the documentation set

## AI Rules
- Never regenerate the whole project.
- Never rewrite existing architecture.
- Never replace technology decisions.
- Never change folder structure.
- Never introduce new frameworks.
- Never introduce microservices.
- Never introduce Kubernetes.
- Never modify architectural decisions.
- Only work on the requested module.
