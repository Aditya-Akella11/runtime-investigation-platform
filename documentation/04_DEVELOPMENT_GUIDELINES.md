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

## Coding Rules
- Write readable and maintainable code
- Keep changes small and focused
- Favor explicit configuration over hidden behavior

## Testing Rules
- Add or update tests for changed behavior
- Prefer integration-style tests for service boundaries
- Avoid fragile test-only code

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
