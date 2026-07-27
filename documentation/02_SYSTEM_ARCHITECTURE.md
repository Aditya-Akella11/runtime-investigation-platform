# 02_SYSTEM_ARCHITECTURE

## High-Level Architecture

Angular

↓

ASP.NET Core API

↓

Runtime Instrumentation Layer

↓

Provider

↓

Application

## System Components

### Frontend
The Angular frontend provides investigation workflows, probe management, audit views, and provider configuration.

### Backend
The ASP.NET Core API hosts investigation services, workflow orchestration, audit logic, and provider integration endpoints.

### Database
The system stores investigations, runtime evidence, probe definitions, provider metadata, and audit records.

### Provider Layer
The provider layer integrates with runtime sources such as Datadog, Dynatrace, and custom runtime agents.

### Future Runtime Agents
The platform is designed to support runtime agents that can automate probe execution, evidence collection, and correlation over time.

## Communication Flow
1. A customer issue or incident creates an investigation.
2. The investigation triggers runtime probes and evidence collection.
3. Provider integrations return telemetry and runtime signals.
4. The backend correlates findings into an investigation record.
5. The frontend presents timeline, evidence, and remediation information.

## Module Responsibilities
- Applications
- Investigations
- Runtime Probes
- Audit
- Provider Integration

## Folder Structure
- frontend/
- backend/
- runtime/
- infrastructure/
- tests/

## Dependency Rules
- Frontend depends on backend APIs.
- Backend depends on runtime instrumentation services and provider adapters.
- Provider integrations should remain isolated behind clear interfaces.

## Security Principles
- Authentication and authorization are required for operational workflows.
- Audit trails must be immutable and traceable.
- Sensitive runtime data should be protected at rest and in transit.

## Deployment Overview
The system should be deployable as containerized services with environment-specific configuration and provider credentials managed securely.
