# Briota Clinical Decision Support System - Database Architecture

## Overview
Clinical Decision Support System (CDSS) for chronic disease management (COPD & CKD). Tracks patient assessments, health metrics, and clinical data across healthcare network serving underserved areas.

## Core Tables

### CDSSResult (132 records)
**Purpose:** COPD patient assessments with health metrics
**Key Columns:**
- Id (PK)
- PatientId (FK)
- DateTime
- CAT_TotalScore (COPD Assessment Test: 0-40)
- CAT_Cough, CAT_Phlegm, CAT_ChestTight, CAT_HillStairs, CAT_LimitedActivity, CAT_ConfLeavingHome, CAT_SleepSoundly, CAT_LotsOfEnergy
- MRC_Scale (Modified Medical Research Council: 0-4)
- Exacerbations_History (count)
- CDSSZone (A, B, C, D - COPD severity)
- TestGrade (classification)
- TechnicianId (FK to AspNetUsers)

### CKDDetails (~50+ records)
**Purpose:** Chronic Kidney Disease tracking
**Key Columns:**
- Id (PK)
- PatientId (FK)
- CreatedDate, UpdatedDate
- HealthScore (0-100 range)
- RiskLevel (0-5 scale)
- RiskAssessment (text)
- Height, Weight, SystollicBloodPressure, DistolicBloodPressure
- ACRValue (Albumin-to-Creatinine Ratio)
- Diagnosis, DiagnosisDate
- PDFReportLink
- CreatedBy, UpdatedBy
- DoctorNotes, Suggestion

### PatientAssessment
**Purpose:** Links assessments to patients and technicians
**Key Columns:**
- Id (PK)
- PatientInfoId (FK to PatientInfo)
- TechnicianId (FK to AspNetUsers, nullable)
- Interpretation (text)
- CreatedDate, UpdatedDate

### AspNetUsers
**Purpose:** Multi-role user system (doctors, technicians, coaches, specialists)
**Key Columns:**
- Id (PK)
- FirstName, LastName
- Email, PhoneNumber
- ClinicRegistrationNumber
- CenterId, DistrictId, StateId, SubCenterId
- IsActive (bit)
- CreatedDate, CreatedBy, UpdatedDate, UpdatedBy
- UniqueId (unique constraint)
- EmployeeNumber (identity)

### Assessment
**Purpose:** Detailed assessment Q&A
**Key Columns:**
- Id (PK)
- PatientAssessmentId (FK)
- AssessmentQuestionId (FK)
- Answer (nvarchar)

### AssessmentQuestion
**Purpose:** Question bank
**Key Columns:**
- Id (PK)
- Question (nvarchar)

## Entity Relationships

```
PatientAssessment (many) ──FK──> PatientInfo (one)
PatientAssessment (many) ──FK──> AspNetUsers/Technician (one)
PatientAssessment (one) ──1:many──> Assessment
Assessment (many) ──FK──> AssessmentQuestion (one)

CKDDetails (many) ──FK──> PatientInfo (one) [cascade delete]
CDSSResult (many) ──FK──> PatientInfo (one)

CDSSResult (many) ──FK──> AspNetUsers/Technician (one, nullable)
```

## Current Performance Issues

### ❌ Missing Indexes
- No index on `DateTime` in CDSSResult → table scan for date-range queries
- No index on `PatientId, DateTime` composite → slow assessment retrieval
- No index on `UpdatedDate` in CKDDetails → slow temporal queries

### ❌ N+1 Query Problem
- Loading PatientAssessment without `.Include(p => p.Technician)` causes multiple DB hits
- Looping through CDSSResults to calculate average HealthScore instead of GROUP BY

### ❌ No Aggregation Queries
- Health score calculations done in C# loops instead of SQL aggregation
- Technician performance metrics require iterating through records

### ❌ Unoptimized Joins
- Assessment → PatientAssessment → PatientInfo = 3 joins without proper indexing
- Risk assessment queries join CKDDetails → PatientInfo without column indexing

## Data Volume

| Table | Record Count | Purpose |
|-------|---------|---------|
| CDSSResult | 132 | COPD assessments |
| CKDDetails | 50+ | Kidney disease tracking |
| PatientAssessment | 100+ | Assessment links |
| Assessment | 80+ | Detailed Q&A |
| AspNetUsers | 50+ | System users |
| PatientInfo | 7000+ | Patient master |

## Date Range in Database
**2023-08-19 to 2025-02-25** (20+ months of real health data)

## Key Insights
1. **Clinical Focus:** System tracks specific health metrics (CAT scores, blood pressure, ACR values)
2. **Multi-role System:** Different users (doctors, technicians, coaches) with role-based relationships
3. **Longitudinal Tracking:** Multiple assessments per patient over time
4. **Complex Hierarchies:** Assessments nested with detailed Q&A, health data linked to assessments
5. **Real-World Complexity:** Nullable foreign keys (orphaned records possible), manual data entry, audit fields
