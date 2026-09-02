# Deployment & Operations Guide: Runtime Investigation Platform

**Version:** Phase 4 Production Release  
**Status:** Approved Deployment Standard  

---

## 1. System Architecture Overview

The Runtime Investigation Platform comprises five loosely coupled subsystems:

1. **Backend API (`RuntimeInvestigation.API`)**: ASP.NET Core 8 Web API managing authentication, investigation aggregate state, probe lifecycle, and bounded result queries.
2. **Runtime Agent (`RuntimeInvestigation.RuntimeAgent`)**: Lightweight in-process / sidecar diagnostic agent orchestrating method resolution, bounded memory buffers, redaction, and CLR Profiling callbacks.
3. **Frontend Dashboard (`frontend`)**: Angular 18 Single-Page Application providing investigation tracking, live probe configuration, and evidence visualization.
4. **Target Demo Application (`PaymentApi` / `SimpleApp`)**: Sample workloads for verifying non-intrusive probe attachment and evidence collection.
5. **MongoDB Persistence Store**: Document database storing investigations, probes, evidence records, and immutable audit logs.

---

## 2. Prerequisites

| Component | Requirement |
|---|---|
| **Operating System** | Linux (Ubuntu 22.04+ / Alpine) or Windows Server 2022+ |
| **Container Engine** | Docker 24.0+ and Docker Compose v2.20+ |
| **Runtime SDK** | .NET SDK 8.0.x and Node.js 20 LTS (for source builds) |
| **Memory Budget** | Minimum 2 GB RAM for container stack; Agent overhead < 50 MB RSS |

---

## 3. Local & Staging Deployment (Docker Compose)

### 3.1 Starting the Stack

```bash
# Clone repository
git clone <repo-url>
cd runtime-investigation-platform

# Start stack in detached mode
docker compose up -d --build
```

### 3.2 Service Endpoints & Health Checks

| Service | Port | Endpoint | Health Route |
|---|---|---|---|
| **API** | `5000` | `http://localhost:5000` | `http://localhost:5000/health/live` |
| **Frontend** | `4200` | `http://localhost:4200` | `http://localhost:4200` |
| **MongoDB** | `27017` | `mongodb://localhost:27017` | Standard Mongo ping |
| **Runtime Agent** | `5005` | `http://localhost:5005` | `http://localhost:5005/health` |

---

## 4. Configuration & Environment Variables

### Backend API Configuration

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://mongodb:27017",
    "DatabaseName": "runtime_investigation",
    "InvestigationsCollectionName": "investigations",
    "ProbesCollectionName": "probes",
    "EvidenceCollectionName": "evidence",
    "AuditCollectionName": "audit_logs"
  },
  "Jwt": {
    "Issuer": "RuntimeInvestigationPlatform",
    "Audience": "RuntimeInvestigationUsers",
    "SigningKey": "REPLACE_WITH_SECURE_256BIT_SECRET_KEY_FOR_PRODUCTION_ENVIRONMENT"
  }
}
```

### Runtime Agent Profiler Environment

When instrumenting a target process via the CLR Profiling API:

```bash
# .NET Core / .NET 8 Targets
export CORECLR_ENABLE_PROFILING="1"
export CORECLR_PROFILER="{23C6D378-9F3C-4364-9842-88E07936E7B6}"
export CORECLR_PROFILER_PATH="/app/agent/CorProfiler.dll"

# .NET Framework 4.7.2+ Targets
export COR_ENABLE_PROFILING="1"
export COR_PROFILER="{23C6D378-9F3C-4364-9842-88E07936E7B6}"
export COR_PROFILER_PATH="C:\\agent\\CorProfiler.dll"
```

---

## 5. Safe Operations & Probe Lifecycle SLA

1. **Default Probe TTL**: Every activated probe automatically expires after a bounded duration (default 30 minutes, maximum 120 minutes).
2. **Background Expiration Service**: `ProbeExpirationService` queries for active expired probes every 5 seconds and initiates structured cleanup.
3. **Data Protection & PII Redaction**: Sensitive parameter names (`password`, `token`, `secret`, `ssn`, `key`) are automatically redacted before entering in-memory buffers.
4. **Buffer Bounds**: In-memory `LogBuffer` drops excess entries with explicit counters when maximum capacity (1,000 entries) is reached, preventing target memory exhaustion.

---

## 6. Troubleshooting & Rollback

- **Probe Fails Activation**: Verify target class and method exist in loaded assemblies and do not reside in protected security namespaces (`System.Security.*`, `Microsoft.AspNetCore.Identity.*`).
- **Emergency Probe Detach**: POST `/api/probes/{id}/deactivate?reason=EmergencyShutdown` immediately dispatches deactivation and frees buffers.
- **Rollback Stack**: `docker compose down -v` cleanly terminates all containers and clears local state.
