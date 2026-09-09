# Sprint 8: Multi-Tenancy & RBAC

## Overview
Sprint 8 delivers enterprise-grade customer data isolation and role-based access control (RBAC). Customer data is isolated across all queries, repositories, and services, and endpoints are guarded with role claims.

---

## Architecture & Data Model

### 1. Domain Entities
- **`TenantUser`** (`RuntimeInvestigation.Domain.Entities.TenantUser`):
  Tenant membership record storing `Id`, `TenantId`, `Email`, `Name`, `Role`, and `CreatedAt`. Compound unique indexing on `(TenantId, Email)`.
- **`UserRole`** (`RuntimeInvestigation.Domain.Entities.UserRole`):
  Enumeration specifying access tiers:
  - `Admin`: Full control (invite users, modify roles, manage investigations and probes).
  - `Investigator`: Operational access (create investigations, activate/deactivate probes, view evidence).
  - `Viewer`: Read-only access (query investigations and probe evidence; 403 on mutation).
- **`Investigation` & `RuntimeProbe`**:
  Updated with `TenantId` (and `CreatedBy` on `Investigation`). All entity creation scopes records to the active tenant.

### 2. Tenancy Context & Security
- **`ITenantContext` / `TenantContext`** (`RuntimeInvestigation.Application.Common.Interfaces`):
  Scoped context extracted from JWT claims (`tenant_id`, `role`, `sub`) or `X-Tenant-Id` header.
- **`TenantContextMiddleware`** (`RuntimeInvestigation.API.Middleware.TenantContextMiddleware`):
  Intercepts authenticated requests, extracts tenant identity, sets `ITenantContext`, and ensures authorized endpoints reject tokens lacking tenant identification with 401 Unauthorized.

### 3. Application & Persistence
- **`UserService` & `IUserRepository`**:
  Supports user provisioning with tenant-level deduplication, role updates, and listing.
- **`MongoUserRepository` & `InMemoryUserRepository`**:
  High-performance persistence with tenant indexing and in-memory mock for integration testing.
- **`InvestigationService` & `ProbeService`**:
  All query and mutation workflows (`GetByIdAsync`, `GetAllAsync`, `UpdateAsync`, `DeleteAsync`) enforce `TenantId` matching to prevent cross-tenant data leakage.

### 4. API & RBAC Matrix
- **`UsersController`** (`/api/users`):
  - `GET /api/users` — `[Authorize(Roles = "Admin,Investigator")]`
  - `POST /api/users` — `[Authorize(Roles = "Admin")]`
  - `PUT /api/users/{id}/role` — `[Authorize(Roles = "Admin")]`
- **`ProbesController`** (`/api/probes`):
  - `POST /api/investigations/{id}/probes` — `[Authorize(Roles = "Admin,Investigator")]`
  - `POST /api/probes/{id}/activate` — `[Authorize(Roles = "Admin,Investigator")]`
  - `POST /api/probes/{id}/deactivate` — `[Authorize(Roles = "Admin,Investigator")]`
  - `GET /api/investigations/{id}/probes` — `[Authorize]` (accessible by Viewer)

---

## Test Verification
- **`TenantIsolationTests`** (Domain.Tests - 6 tests):
  - Entity tenant properties, context updates, repository isolation, and cross-tenant hiding via `InvestigationService`.
- **`RbacAuthorizationTests`** (API.Tests - 6 tests):
  - Unauthorized (401) without token, Forbidden (403) for Viewer mutating probes or users, and Created (201) for Admin.
- **`UserServiceTests`** (Application.Tests - 5 tests):
  - User creation, duplicate prevention per tenant, cross-tenant email reuse, tenant listing, and role updates.
- **Full Solution**: 151 passing tests, 0 failures.
