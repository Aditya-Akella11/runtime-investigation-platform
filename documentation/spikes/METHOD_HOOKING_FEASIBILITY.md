# Technical Spike: Cross-Process CLR Method Hooking & ReJIT Feasibility

**Date:** 2026-09-01  
**Sprint:** Phase 4, Sprint 2 (Ticket 11)  
**Status:** Approved Technical Spike  

---

## 1. Executive Summary

Cross-process instrumentation in modern .NET (.NET Core 3.1+, .NET 6/7/8/9) and legacy .NET Framework 4.7.2+ cannot safely rely on in-memory binary patching or arbitrary function-pointer redirection (`JMP` insertion). These mechanisms fail across Tiered Compilation, Dynamic PGO, JIT Inlining, and W^X memory protections.

The approved production path uses the **CLR Profiling API** (`ICorProfilerCallback` + `ICorProfilerInfo` / `RequestReJIT`) with structured environment configuration and fail-closed safety boundaries.

---

## 2. Evaluation of Interception Mechanisms

| Mechanism | Safety | Reversibility | Optimized/JIT Tiering | Async Support | Production Feasibility |
|---|---|---|---|---|---|
| **Raw Trampoline / Native Patching (`PAGE_EXECUTE_READWRITE`)** | ❌ Extreme crash risk | ❌ Hard to rollback safely under concurrency | ❌ Fails on JIT re-compilation / OSR | ❌ Breaks async state machines | ⛔ Prohibited |
| **`DynamicMethod` IL Wrapper** | ✅ High | ✅ Full garbage collection | ⚠️ In-Process Only | ✅ Full | ✅ In-process testing & spike harness |
| **CLR Profiling API (`SetILFunctionBody` & `RequestReJIT`)** | ✅ Highest | ✅ Supported via ReJIT revert | ✅ Supported (JIT-aware) | ✅ Preserves state machines | 🌟 Production Recommendation |

---

## 3. Architecture & Requirements

1. **Architecture Neutrality**: Profiler binaries must match the target process bitness (x64 / ARM64 / x86).
2. **Memory Safety**: No arbitrary executable memory allocation. All metadata/IL rewrite operations must use `IMetaDataEmit` and `SetILFunctionBody` provided by the runtime.
3. **Rollback & Deactivation**: On probe deactivation or expiration, `RequestReJIT` requests the original method IL, ensuring zero-overhead baseline execution after probe cleanup.
4. **Async & Exception Handling**: State machine methods (`MoveNext`) are instrumented at the IL entry/exit points, preserving `Task` completion status and exception propagation.
