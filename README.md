# Runtime Investigation Platform

This repository contains the initial foundation for a Runtime Investigation Platform designed to let engineers add temporary runtime instrumentation to production systems without changing source code or deploying a new version.

## Product Focus

This is not another observability, logging, or monitoring platform. It is an investigation-first platform focused on:

- creating investigations from production issues
- creating temporary runtime probes
- collecting runtime evidence safely
- finding root cause quickly
- removing instrumentation automatically after use

The MVP is demo-first: a controlled payment application with intentional defects proves the complete workflow before the real .NET agent is built.

## Architecture First Approach

The implementation must stay aligned with the architectural documents in the documentation/ folder. Those documents are the single source of truth and should not be changed without explicit approval.

## Project Structure

- documentation/ - architectural and governance documents
- frontend/ - Angular client application
- backend/ - ASP.NET Core API and modular monolith services
- runtime/ - runtime instrumentation and probe components
- providers/ - provider integration adapters
- agents/ - future and current runtime agent assets
- infrastructure/ - deployment and environment assets
- scripts/ - automation scripts
- tests/ - automated tests
- demo/ - controlled demo applications and scenarios

## Architectural Documents

- documentation/01_PRODUCT_VISION.md
- documentation/02_SYSTEM_ARCHITECTURE.md
- documentation/03_TECH_STACK_AND_STANDARDS.md
- documentation/04_DEVELOPMENT_GUIDELINES.md
- documentation/05_PROJECT_ROADMAP.md
- documentation/06_EPIC_1_IMPLEMENTATION_TRACKER.md

## Current Development Direction

The immediate goal is to prove the complete probe lifecycle with a mock agent and a controlled demo Payment API: create, persist, dispatch, activate, collect evidence, audit, and expire. The first production runtime agent will target .NET and a narrow set of probes. Java, Python, and Node are deferred until the .NET workflow is safe, measurable, and validated with users.

The runtime agent - not Angular, MongoDB, or the API framework - is the intended technical moat. For MVP validation, the user experience is proven first through a controlled demo Payment API and a mock agent. See the architecture and roadmap documents for the safety constraints and staged delivery plan.
