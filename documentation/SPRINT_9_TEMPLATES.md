# Sprint 9 — Investigation Templates & Approval Workflow

**Branch:** `sprint-9-templates`  
**Tickets:** #76–87  
**Story Points:** 24  
**Status:** ✅ Complete

---

## Overview

Sprint 9 introduces two enterprise-grade features:

1. **Investigation Templates** — pre-built probe configurations that allow teams to instantiate a fully configured investigation with one click, eliminating manual probe setup for common scenarios.
2. **Approval Workflow** — a `Draft → PendingApproval → Active | Rejected` state machine that enforces a governance gate before any probe is deployed to production.

---

## Template System

### Domain Entities

| Entity | Location | Purpose |
|--------|----------|---------|
| `InvestigationTemplate` | `Domain/Entities/` | Root entity: Name, Category, Description, list of `ProbeTemplate` |
| `ProbeTemplate` | `Domain/Entities/` | Value object: ProbeType, Target, Expression, Condition, DurationMinutes |

### Application Layer

| Class | Location | Key Methods |
|-------|----------|-------------|
| `ITemplateRepository` | `Application/Features/Templates/` | `GetAllAsync()`, `GetByIdAsync()`, `AddAsync()` |
| `TemplateService` | `Application/Features/Templates/` | `ListTemplatesAsync()`, `CreateFromTemplateAsync()` |
| `TemplateContracts` | `Application/Features/Templates/` | `TemplateDto`, `CreateFromTemplateCommand`, `TemplateInstantiationResult` |

### Infrastructure Layer

| Class | Location | Purpose |
|-------|----------|---------|
| `MongoTemplateRepository` | `Infrastructure/Persistence/Repositories/` | MongoDB-backed template storage |
| `InMemoryTemplateRepository` | `Infrastructure/Persistence/Repositories/` | In-memory implementation (used in tests) |
| `TemplateSeeder` | `Infrastructure/Persistence/` | Seeds 3 built-in templates on startup |

### Built-in Templates

| Template | Category | Probes | Use Case |
|----------|----------|--------|---------|
| **Debug N+1 Query** | Performance | 2 (Log + Snapshot) | ORM producing too many DB queries |
| **Authentication Failures** | Security | 2 (Log + Log) | Repeated login failures, brute-force attempts |
| **Memory Leak Investigation** | Reliability | 2 (Metric + Snapshot) | Growing heap, OOM risk |

### API

```
GET  /api/templates                      → List all templates
GET  /api/templates/{id}                 → Get single template
POST /api/templates/instantiate          → Create investigation + probes from template
```

---

## Approval Workflow

### State Machine

```
         ┌──────────────┐
         │    Draft     │  ← default on create
         └──────┬───────┘
                │ SubmitForApproval()
                ▼
      ┌──────────────────┐
      │  PendingApproval  │
      └───┬──────────┬───┘
          │          │
   Approve()      Reject(reason)
          │          │
          ▼          ▼
       ┌──────┐  ┌──────────┐
       │Active│  │ Rejected │
       └──────┘  └──────────┘
                      │
               SubmitForApproval()
                      │
                      ▼
             PendingApproval (re-submit)
```

### Investigation Entity Changes

| Property | Type | Description |
|---------|------|-------------|
| `ApprovalStatus` | `ApprovalStatus` enum | Current state: Draft / PendingApproval / Active / Rejected |
| `ApprovedBy` | `string?` | Admin ID who approved |
| `RejectionReason` | `string?` | Free-text reason for rejection |

#### Domain Methods (guarded transitions)

```csharp
investigation.SubmitForApproval();           // Draft/Rejected → PendingApproval
investigation.Approve(adminId);             // PendingApproval → Active
investigation.Reject(adminId, reason);      // PendingApproval → Rejected
```

### Application Layer — ApprovalService

| Method | Role | Effect |
|--------|------|--------|
| `SubmitForApprovalAsync(id)` | Investigator | Moves Draft/Rejected to PendingApproval |
| `ApproveAsync(id, adminId)` | Admin | Moves to Active |
| `RejectAsync(id, adminId, reason)` | Admin | Moves to Rejected with reason |

### API

```
POST /api/investigations/{id}/submit   → Submit for approval
POST /api/investigations/{id}/approve  → Approve (Admin role)
POST /api/investigations/{id}/reject   → Reject with reason body
```

---

## Frontend

### template.service.ts

Angular service (`src/app/core/`) exposing:

```typescript
getTemplates(): Observable<InvestigationTemplateDto[]>
getTemplate(id): Observable<InvestigationTemplateDto>
createFromTemplate(command): Observable<TemplateInstantiationResult>
```

### template-selector.component.ts

Two-step modal dialog:
1. **Step 1** — grid of template cards (name, category badge, description, probe count chips)
2. **Step 2** — fill in Application ID, optional title/description override

Accessible (role="dialog", aria-modal, keyboard support), self-contained CSS.

### investigation-detail.component.ts

Updated to show:
- **Approval status badge** next to investigation status
- **Submit for Approval** button — visible when status is Draft or Rejected
- **Approve / Reject** buttons — visible when status is PendingApproval
- `approvedBy` / `rejectionReason` meta fields

---

## Tests Added

| File | Project | Tests | Coverage |
|------|---------|-------|---------|
| `InvestigationApprovalTests.cs` | Domain.Tests | 8 | State-machine guard logic |
| `TemplateSeederTests.cs` | Domain.Tests | 3 | 3 templates exist, each has probes |
| `TemplateServiceTests.cs` | Application.Tests | 6 | List, create-from-template, deep-copy |
| `ApprovalServiceTests.cs` | Application.Tests | 6 | Submit, approve, reject, invalid transitions |

**Total new tests:** 23  
**Cumulative total:** 177 passing, 0 failing

---

## How to Verify

### API via curl

```bash
# List built-in templates
curl http://localhost:5000/api/templates

# Instantiate from first template
curl -X POST http://localhost:5000/api/templates/instantiate \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{"templateId":"<id>","applicationId":"PaymentApi","title":"N+1 Debug Run 1"}'

# Submit the resulting investigation for approval
curl -X POST http://localhost:5000/api/investigations/<inv-id>/submit \
  -H "Authorization: Bearer <token>"

# Approve
curl -X POST http://localhost:5000/api/investigations/<inv-id>/approve \
  -H "Authorization: Bearer <admin-token>"
```

### UI Flow

1. Navigate to **Investigations** → click **⚙ From Template**
2. Select "Debug N+1 Query" template → click Next
3. Enter Application ID → click **Create Investigation**
4. On the investigation detail page, click **Submit for Approval**
5. (As Admin) click **✓ Approve Investigation** — status changes to Active

---

## Related Documents

- [`SPRINT_7_ADVANCED_PROBES.md`](./SPRINT_7_ADVANCED_PROBES.md)
- [`SPRINT_8_MULTI_TENANCY.md`](./SPRINT_8_MULTI_TENANCY.md)
- [`11_PHASE_5_EXECUTION_GUIDE.md`](./11_PHASE_5_EXECUTION_GUIDE.md)
