# Copilot Instructions

IMPORTANT

The repository contains the architectural documents in documentation/.
These documents are the single source of truth.
Do NOT modify them.
Do NOT reinterpret them.
Do NOT replace architectural decisions.
Do NOT introduce different technologies.
Do NOT redesign the architecture.
Do NOT introduce microservices.
Do NOT introduce Kubernetes.
Do NOT change folder structure.
Do NOT rename modules.
Do NOT change API contracts.
Do NOT change MongoDB collections.
Do NOT introduce additional frameworks unless explicitly requested.
If implementation requires changing one of these documents, stop and explain the reason.
Wait for explicit approval before changing any architecture.

## Project Scope

This platform is an investigation-first system for temporary runtime instrumentation. It must support safe production changes, automatic cleanup, vendor-neutral integration, and future runtime agent automation.

## Working Style
- Keep changes scoped to the requested module.
- Prefer incremental implementation over large rewrites.
- Follow the documented tech stack and development guidelines.
- Preserve the existing folder structure.
- Build one module at a time and keep each feature production ready.
- Never implement future roadmap items unless explicitly requested.
