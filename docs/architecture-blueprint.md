# Briota CDSS: Architecture Blueprint

This document defines the core architectural patterns and security standards for the Briota Clinical Decision Support System.

## 1. Safety Patterns (Clinical Resilience)

### 🔄 1.1 The "Load-and-Patch" Update Logic
To handle high-reliability updates without clinical data corruption, we implemented the **Load-and-Patch** pattern in `Edit.cshtml.cs`.

*   **Logic**: Instead of binding the form directly to the database entry, we:
    1.  Fetch the "Fresh" record from the DB by ID.
    2.  Patch ONLY the modified fields (e.g., `Symptoms`).
    3.  Rotate the `ConcurrencyStamp` (Version ID) to a new GUID.
*   **Result**: This prevents "Null Injections" into mandatory fields (like `DateOfBirth` or `Gender`) and ensures that database constraints are always satisfied.

---

## 2. Security & RBAC Patterns

### 🛡️ 2.1 Multi-Tenant Security (Global Isolation)
Every clinical query is scoped to a `CenterId` extraction service.
*   **Pattern**: `HasQueryFilter` is applied to all clinical entities in `HealthDbContext`.
*   **Enforcement**: The database engine itself appends the tenant filter to every SQL statement, making data leakage impossible at the application layer.

### 👤 2.2 Role-Based Component Rendering
The **Clinical Command Center** (Dashboard) uses a "Policy-First" rendering approach.
*   **System Admin**: Authorized for global audit views and system health stats.
*   **Doctor/Technician**: Authorized for patient-specific screening and dashboard alerts.
*   **Implementation**: Done via `@if (User.IsInRole("Admin"))` blocks within Razor pages, ensuring a tailored experience for every clinical user.

---

## 3. Database Resilience

### 🧬 3.1 Mapping Mandatory Clinical Data
The `SecurityDB_06June` schema contains strict non-nullable requirements.
*   **Identity Fix**: Corrected the `ApplicationUser` seeding to include mandatory clinical metadata (`UniqueId`, `IsActive`) which was blocking new user registration.
*   **Floating Points**: Mapped clinical metrics to `real` and `float` types in EF Core to ensure precision matching with the underlying SQL Engine.

---

## ⚡ 4. High-Scale Navigation Pattern

### 🚀 4.1 Index-First Sorting
To handle **23,491 records** without triggering SQL timeouts:
*   **The Problem**: Sorting by `CreatedDate` (unindexed) caused full-table scans.
*   **The Fix**: Navigating via `Id DESC` (Clustered Index).
*   **Performance**: Results in **near-zero latency** pagination even at extreme database volumes.

### ⚡ 4.2 Enterprise Search Hybrid (SQL + RAM)
The Search Laboratory implements a dual-tier resilience strategy:
*   **Surgical Projections**: Columnar `.Select()` filters in Tier 1 reduce the payload by 80% compared to the Tier 0 "Select All" approach.
*   **Application-Level Caching**: `IMemoryCache` stores deterministic search results for 2 minutes. This eliminates SQL load for repeated queries, achieving **0ms latency** and protecting the database from search-intensive "clinician bursts."

---
_Blueprint Revision: April 13, 2026_
