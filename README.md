# Runtime Investigation Platform

This repository contains the initial foundation for a Runtime Investigation Platform designed to let engineers add temporary runtime instrumentation to production systems without changing source code or deploying a new version.

## Product Focus

This is not another observability, logging, or monitoring platform. It is an investigation-first platform focused on:

- creating investigations from production issues
- creating temporary runtime probes
- collecting runtime evidence safely
- finding root cause quickly
- removing instrumentation automatically after use

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

## Architectural Documents

- documentation/01_PRODUCT_VISION.md
- documentation/02_SYSTEM_ARCHITECTURE.md
- documentation/03_TECH_STACK_AND_STANDARDS.md
- documentation/04_DEVELOPMENT_GUIDELINES.md
- documentation/05_PROJECT_ROADMAP.md

## Current Development Direction

Build the platform incrementally, one module at a time. Each module should be production-ready before moving to the next.
