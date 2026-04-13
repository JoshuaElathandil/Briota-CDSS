# 🧬 Briota CDSS: Technical Evolution Tiers

This document defines the architectural journey from an experimental "Baseline" state to the "Production Hardened" clinical platform. This taxonomy provides the technical narrative for the project's evolution.

---

## 🛑 Tier 0: The Baseline Prototype (The "Before")

The Baseline Prototype represents the system prior to architectural hardening. It was characterized by an **"Implicit Trust"** model — a common pitfall in junior-level development where the system assumes the happy path is the only path.

### Key Characteristics:
*   **Performance**: Relied on **Linear Scanning**. Unindexed `Contains()` queries resulted in full table scans, capping the system’s utility at ~100 records before connection timeouts began.
*   **Security**: Minimal isolation. The system lacked global tenant enforcement, creating a high risk of cross-center data leakage.
*   **Integrity**: **Non-Atomic Updates**. Saving data used the "Last-In-Wins" approach, leading to silent data loss in multi-clinician environments.
*   **Resilience**: High failure rate. Accessing deleted medical relations or inputting outlying medical data would cause immediate **System Crashes (500 Internal Server Error)**.

---

## 🏆 Tier 1: Production Hardened System (The "After")

The Production Hardened system represents a complete architectural rebuild focusing on **"Defensive Clinical Engineering."** It is designed for enterprise scale, data immutability, and clinical reliability.

### Key Assets:
*   **Performance**: **Index-First Execution**. Utilizing non-clustered SQL indexes and SARGable `StartsWith()` predicates, handling **23,000+ records** with sub-2s response times.
*   **Security**: **Global Isolation Layers**. Multi-tenancy is enforced at the DB Context level via Global Query Filters, making data leakage impossible by design.
*   **Integrity**: **Optimistic Concurrency Control (OCC)**. Implements a "Load-and-Patch" strategy with version rotation to protect every clinical data point.
*   **Resilience**: **Graceful Resilience**. Uses null-coalescing fallbacks and domain range validation to ensure the clinical UI remains stable even when facing corrupt or missing metadata.

---

## 🏛️ Architectural Verdicts

| Dimension | Winston's Verdict (Architect) | Dr. Quinn's Diagnosis (Problem Solver) |
| :--- | :--- | :--- |
| **Search** | "Linear scanning is an unscalable debt." | "AHA! Full scans are code-level nearsightedness." |
| **Concurrency**| "Silent overwrites cause atomic data decay." | "The Last-In-Wins bug is a ghost in the machine." |
| **Reliability** | "Implicit trust is a clinical liability." | "The prototype followed the happy path to a cliff." |

---
_Reference: Documentation Hub / architecture-blueprint.md_
