# Briota CDSS: Administration Laboratory Guide

This guide details the technical operation of the integrated laboratories designed to demonstrate system resilience and clinical safety.

## 🧪 1. Search Performance Laboratory
**Location**: `/Admin/Lab/Search`

This laboratory demonstrates the architectural leap from a prototype "Tier 0" to an enterprise "Tier 1" search path.

### Tier 0: The "Baseline" Pattern
- **Logic**: Performs a full linear table scan on concatenated strings: `(p.FirstName + " " + p.LastName).Contains(query)`.
- **The Problem**: This is non-SARGable and ignores database indexes. At 23,000+ records, this causes significant disk I/O and latency.
- **Latency**: ~50s (extreme) or ~180ms (standard SQL fetch).

### Tier 1: The "Hardened" Pattern
- **Logic**: SARGable columnar filtering: `p.FirstName.Contains(query) || p.LastName.Contains(query)`.
- **Lean Projections**: It uses `.Select()` to pull only the 6 required columns, reducing network payload by ~80%.
- **Memory Caching**: Implements `IMemoryCache`. 
    - **First Fetch**: ~13ms (SQL indexed).
    - **Subsequent Fetches**: **0 ms** (RAM).
- **Presentation**: Use this to show how "Enterprise Software" stays responsive even when the database is under load.

---

## 🔒 2. Concurrency Laboratory
**Location**: `/Admin/Lab/Concurrency`

Demonstrates the **Load-and-Patch** and **Optimistic Concurrency** patterns used to protect clinical data integrity.

### The "Virtual Terminal" Setup
The UI simulates two doctors (Terminal A and Terminal B) accessing the same patient record concurrently.

### Conflict Detection (Tier 1)
1. **Load**: Both terminals load the record with `ConcurrencyStamp` "X".
2. **Action**: Doctor A saves a change. The system rotates the stamp to "Y".
3. **Collision**: Doctor B tries to save using the now-stale "X" stamp.
4. **Resolution**: EF Core throws a `DbUpdateConcurrencyException`. The update is rejected, and Doctor B is alerted to clinical data drift.

### Validation Sequence
- **Step 1**: Load the lab in Tier 1.
- **Step 2**: Edit symbols in Terminal A and click **Save**. Observe the success message.
- **Step 3**: Without refreshing, edit symbols in Terminal B and click **Save**. Observe the **CRITICAL CONFLICT** rejection.

---

## 📈 3. System Stability Laboratory
**Location**: `/Admin/Lab/Stability`

Used to verify the underlying platform health and connection resilience.

- **Timeout Verification**: Proves the 180s command timeout is active for long-running clinical audits.
- **Tenant Scope**: Verifies that the Global Query Filter is correctly isolating data for the active clinic.

---
_Documentation curated by the CDSS Hardening Team_
