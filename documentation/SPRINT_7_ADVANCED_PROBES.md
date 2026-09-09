# Sprint 7: Advanced Probes (Snapshot, Metric, Conditions)

## Overview
Sprint 7 extends the dynamic instrumentation capabilities of the Runtime Investigation Platform by introducing:
1. **Snapshot Probes**: Capture argument/local variable states when a probed method executes without breaking execution flow.
2. **Metric Probes**: Low-overhead thread-safe metric counters measuring invocation frequency, cumulative execution duration, and exception occurrences.
3. **Condition Evaluator**: Dynamic predicate filtering syntax (`args[n] > value`, `args[n] == value`, `args[n] != null`) to capture only high-value telemetry.

---

## Architecture & Components

### 1. Domain Entities
- **`ConditionEvaluator`** (`RuntimeInvestigation.Domain.Entities.ConditionEvaluator`):
  Parses and evaluates simple comparison expressions against invocation argument arrays. Supports numeric comparison (`>`, `<`, `>=`, `<=`, `==`, `!=`), null checks (`== null`, `!= null`), and string equality.
- **`SnapshotCapture`** (`RuntimeInvestigation.Domain.Entities.SnapshotCapture`):
  Immutable record representing a snapshot capture containing `ProbeId`, UTC timestamp, and a dictionary of captured variable values.
- **`MetricCounters`** (`RuntimeInvestigation.Domain.Entities.MetricCounters`):
  Tracks runtime metrics: `CallCount`, `TotalDurationMs`, `ExceptionCount`, `AverageDurationMs`, and `LastUpdated`.

### 2. RuntimeAgent Instrumentation
- **`SnapshotCollector`** (`RuntimeInvestigation.RuntimeAgent.Instrumentation.SnapshotCollector`):
  Collects argument values at probe entry point, conditionally filtered through `ConditionEvaluator`.
- **`MetricCollector`** (`RuntimeInvestigation.RuntimeAgent.Instrumentation.MetricCollector`):
  Employs `Interlocked` atomic primitives and lightweight `Stopwatch` scopes to maintain <2% overhead budget during performance profiling.

### 3. API & Contracts
- **`ProbesController`** (`RuntimeInvestigation.API.Controllers.ProbesController`):
  - `GET /api/probes/{id}/snapshot` — returns latest variable snapshot.
  - `GET /api/probes/{id}/metrics` — returns aggregated invocation counts and execution durations.
  - `POST /api/investigations/{id}/probes` — accepts optional `condition` string parameter.

---

## Test Verification
- **`ConditionEvaluatorTests`**: 8 unit tests covering null expressions, numerical comparisons, string comparisons, null values, and syntax errors.
- **`SnapshotCollectorTests`**: 5 unit tests verifying argument capture, condition filtering, null arguments, and clearing buffers.
- **`MetricCollectorTests`**: 5 unit tests validating call count tracking, timing accuracy, exception counts, and concurrent thread-safety.
- **Full Solution**: 134 passing tests, 0 failures.
