# Briota CDSS: Research & Discovery Distillate

This document synthesizes the architectural research and domain knowledge gathered from the `TableInfo` schema and the `OLDWORK` reference.

## 1. Schema Synthesis
The system operates on the `SecurityDB_06June` database (137 tables). Our project centers on 7 core tables:

*   **`AspNetUsers`**: Central entity using `nvarchar(900)` for IDs. Contains `CenterId` which is our primary security isolation key.
*   **`CKDDetails`**: The main clinical data bucket. Contains 40+ specialized columns including symptoms (`bit`), vitals (`real`/`float`), and AI scores (`real`).
*   **`PatientAssessment`**: Links patients to technicians and specific diagnostic sessions.
*   **`Assessment` / `AssessmentQuestion`**: Dynamic clinical questionnaire engine.

## 2. The Legacy Shift: OLDWORK vs. NEW CDSS
We have moved away from the "String Parsing" model found in the `OLDWORK` sample.

| Feature | Legacy (OLDWORK) Pattern | New (CDSS) Pattern |
| :--- | :--- | :--- |
| **Data Integrity** | `ReportText` blob (manual string split) | **Typed Columns**: `Diagnosis`, `Medication`, `DoctorNotes`. |
| **User Identity** | Local `User` table with `int` IDs. | **Identity Integration**: `AspNetUsers` with `nvarchar(900)`. |
| **Reporting** | Syncfusion Grid (Static) | **Syncfusion + DTOs**: Categorized views (Symptoms vs. Metrics). |
| **Scale** | Synchronous requests (`.ToList()`) | **Async Architecture**: `ToListAsync()` and `CancellationToken` support. |

## 3. The 6 Industry-Standard Bug Stories
These scenarios represent the technical debt we are proactively preventing:

1.  **Bug #1: The N+1 Shadow**: Fetching `Technician` names row-by-row in assessment lists. *Prevention: `.Include()`*.
2.  **Bug #2: The Clinic Cross-Talk (IDOR)**: Technicians accessing patients from other `CenterIds`. *Prevention: Global Query Filters*.
3.  **Bug #3: The Vanishing Vital (Concurrency)**: Doctors overwriting each other's notes. *Prevention: `ConcurrencyStamp` handling*.
4.  **Bug #4: Semantic Validation Bypass**: Negative weight or impossible BP values. *Prevention: `FluentValidation`*.
5.  **Bug #5: Thread Hunger**: Sync I/O blocking the Web API. *Prevention: 100% `async` stack*.
6.  **Bug #6: Fragmented View**: Clinical metadata hidden in 40+ flat columns. *Prevention: Categorized DTOs*.

## 4. Key Discovery: The "nvarchar(900)" Factor
The database uses `nvarchar(900)` for key fields in the Identity tables. This is a non-standard length (usually 450).
*   **Impact**: EF Core must be explicitly told to use this length, or it will generate sub-optimal SQL that bypasses indexes.
*   **Fix**: `.HasMaxLength(900)` explicitly in `OnModelCreating`.
