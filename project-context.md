# Project Context: Briota CDSS Platform

## Project Soul
Briota CDSS is a high-performance, professional-grade Clinical Decision Support System designed to analyze cross-disease metadata (CKD, Diabetes, COPD) using ASP.NET Core and SQL Server. The system emphasizes clinical data integrity, AI-driven diagnostics, and secure multi-tenant access for clinics (Centers).

## Expert Roster & Principles

| Name | Role | Icon | Principle |
| :--- | :--- | :--- | :--- |
| **Winston** | Architect | 🏗️ | "Lean architecture: Typed columns over string-parsing blobs." |
| **Amelia** | Developer | 💻 | "100% Async/Await: No blocking the thread pool for clinical reports." |
| **Sally** | UX Designer | 🎨 | "Visual High-Fidelity: Medical status must be scannable in 3 seconds." |
| **John** | Product Manager | 📋 | "Data Richness: Surface the AI predictions that add clinical value." |

## Technical Environment & Rules

### Tech Stack
*   **Framework**: ASP.NET Core 9.0.x
*   **ORM**: Entity Framework Core (SQL Server)
*   **Database**: `SecurityDB_06June` (SSMS - 137 Tables)
*   **UI Framework**: Razor Pages + **DataTables.net** + **Chart.js** (Zero-Fee Stack)

### Critical Implementation Rules
1.  **NO LEGACY PARSING**: Do not use the `OLDWORK` approach of parsing a `ReportText` string into diagnosis/medication. Use the typed columns in `CKDDetails` (e.g., `DoctorNotes`, `Diagnosis`).
2.  **CENTER ISOLATION**: Every clinical query MUST include a filter for `CenterId` derived from the `AspNetUser` context to prevent cross-clinic data leakage (BOLA/IDOR).
3.  **STRICT MAPPING**: ASP.NET Identity tables in this DB use `nvarchar(900)` for Primary/Foreign Keys. You MUST specify this in Fluent API to ensure index hits.
4.  **RECOVERY PATTERN**: Use `IDesignTimeDbContextFactory` (as seen in `OLDWORK`) to handle local migrations against the production-ready schema.
5.  **ASYNC FIRST**: All database interactions must use `async`/`await`.

## Reference Folders
*   `docs/TableInfo/`: High-fidelity schema exports for the 7 core clinical tables.
*   `Briota.CDSS/`: The active high-fidelity implementation (Use this as the source of truth).
*   `OLDWORK/`: Reference only for the "Login flow". **DO NOT** copy logic or commercial components from `OLDWORK`.

## Success Criteria (Day 4)
*   Single query retrieval for patient assessments (N+1 resolved).
*   Correct medical range validation for `real` and `float` metrics.
*   "Conflict-handled" updates for clinical records via `ConcurrencyStamp`.
