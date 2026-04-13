# Viva Preparation: Talking Points & Q&A

## Opening Statement (1 minute)

**What to Say:**

*"I developed a Clinical Decision Support System for managing chronic diseases like COPD and CKD. The system tracks patient assessments, clinical metrics, and health data across a healthcare network serving underserved areas. During the internship, my focus was on optimizing database query performance and implementing robust error handling to make the platform more reliable and responsive for healthcare providers."*

**Key Points to Emphasize:**
- Real healthcare system (not just a demo project)
- Multi-disease support (COPD + CKD)
- Patient data tracking over time
- Performance optimization (your main contribution)
- Reliability focus (error handling)

---

## Topic 1: Project Architecture & Database (2 minutes)

**Question:** "Walk us through the database architecture of your system."

**Answer:**

*"The system uses SQL Server 2022 and is built around 5 core tables. CDSSResult tracks COPD assessments with CAT scoring and MRC scale measurements. CKDDetails stores chronic kidney disease data including blood pressure, weight, and health scores. PatientAssessment links these assessments to patients and technicians. Assessment and AssessmentQuestion handle detailed questionnaires. Finally, AspNetUsers manages the multi-role system with doctors, technicians, and coaches. The database contains over 200 real health records spanning from August 2023 to February 2025."*

**Key Diagram to Explain:**
```
CDSSResult (COPD) ──┐
                    ├──> PatientAssessment <── AspNetUsers (Technician)
CKDDetails (CKD) ──┘
                    └──> Assessment ──> AssessmentQuestion
```

**Talking Points:**
- Covers two major chronic diseases
- Multi-user system with role-based relationships
- Longitudinal data (tracks patients over time)
- Real clinical data with structured assessments

---

## Topic 2: Query Optimization (2 minutes)

**Question:** "Tell me about a performance optimization you made."

**Answer:**

*"The system had a performance problem: retrieving COPD assessments by date range took 450 milliseconds. When I analyzed the SQL execution plan, I discovered the query was doing a table scan—scanning all 132 rows and checking each one against the filter criteria. To fix this, I created a composite index on (PatientId, DateTime DESC) with an INCLUDE clause that brought the frequently accessed columns into the leaf pages. After creating the index, the same query now executes in just 12 milliseconds. That's a 37x improvement, with logical reads dropping from 50+ down to 2-3."*

**How to Demonstrate:**

1. **Show SSMS Execution Plans Side-by-Side:**
   - BEFORE: Table Scan (Estimated Subtree Cost: 0.35)
   - AFTER: Index Seek (Estimated Subtree Cost: 0.003)

2. **Show Query Times:**
   - BEFORE: 450ms
   - AFTER: 12ms

3. **Show Logical Reads:**
   - BEFORE: SET STATISTICS IO ON
   - AFTER: (2-3 logical reads vs 50+)

**SQL Command to Show:**
```sql
-- Create index
CREATE NONCLUSTERED INDEX IX_CDSSResult_PatientId_DateTime
ON CDSSResult (PatientId, DateTime DESC)
INCLUDE (CAT_TotalScore, MRC_Scale, HealthScore);

-- Run query to show new performance
SELECT * FROM CDSSResult 
WHERE PatientId = 2 
AND DateTime >= '2024-01-01' 
AND DateTime <= '2024-12-31';
```

**Follow-Up Talking Points:**
- Why composite index? "PatientId first because we filter by patient, then DateTime DESC for chronological ordering"
- Why INCLUDE clause? "Brings all needed columns into index leaf pages, eliminating key lookups"
- Trade-off? "Indexes slow down INSERT/UPDATE slightly, but for a read-heavy healthcare system, that's acceptable"

---

## Topic 3: Bug Fixes & Error Handling (2-3 minutes)

**Question:** "Tell me about bugs you found and fixed."

**Answer #1: Null Reference Exception**

*"The system crashed with null reference exceptions when loading assessments. The root cause was that the TechnicianId foreign key was nullable—when a technician was deleted or reassigned, trying to access the Technician object caused a crash. I fixed this three ways: First, I used .Include() to eagerly load the Technician data in the same query, avoiding lazy loading issues. Second, I checked if the assessment was null before accessing it. Third, I used the null-coalescing operator (??) to provide a default value like 'Unassigned' if no technician existed. This prevents crashes and provides better user experience."*

**Code to Show:**
```csharp
// FIXED VERSION
var assessment = await _db.PatientAssessments
  .Include(p => p.Technician)  // Eager load
  .FirstOrDefaultAsync(p => p.Id == id);

if (assessment == null) return NotFound();

var technicianName = assessment.Technician?.FirstName ?? "Unassigned";
```

**Answer #2: Invalid Health Score**

*"Health scores in the CKDDetails table were sometimes saved with values like -50 or 150, when they should only be 0-100. There was no validation. I implemented validation at multiple layers: First, data annotations on the model with Range(0, 100). Second, explicit validation in the controller before saving. Third, a database CHECK constraint to prevent invalid data at the SQL level. This ensures data quality regardless of whether data comes through the API or direct SQL."*

**Code to Show:**
```csharp
// Model validation
[Range(0, 100, ErrorMessage = "HealthScore must be 0-100")]
public int HealthScore { get; set; }

// Controller validation
if (dto.HealthScore < 0 || dto.HealthScore > 100)
  return BadRequest("Health score must be 0-100");
```

**Answer #3: Orphaned Records**

*"Assessment records could exist in the database without valid PatientAssessment references. I fixed this by checking that all referenced records exist before creating new ones. I also added transaction support so if any validation fails, the entire operation rolls back. This ensures referential integrity is enforced at the application level."*

**Key Phrase:**
> "I enforced referential integrity at the application layer, not just at the database level."

---

## Topic 4: Technology & Tools Used (1-2 minutes)

**Question:** "What technologies did you use in your project?"

**Answer:**

*"The backend is built with ASP.NET Core 8 using Entity Framework Core for ORM. The database is SQL Server 2022 with SQL Server Management Studio for query analysis and optimization. For the frontend, I used HTML, JavaScript, and Razor Pages for a simple dashboard. The key tools for optimization were SQL Server's built-in execution plan analyzer and profiler. I used Visual Studio for development and Postman for API testing."*

**Key Technologies to List:**
- **Backend:** ASP.NET Core 8, Entity Framework Core
- **Database:** SQL Server 2022, SSMS
- **Frontend:** Razor Pages, HTML, JavaScript
- **Testing:** Postman, Unit Tests
- **Tools:** Visual Studio, Execution Plans, Profiler

---

## Expected Q&A

### Q1: "How did you identify the performance bottleneck?"
**A:** "By analyzing SQL execution plans in SSMS. The execution plan showed a table scan with high estimated cost. Once I identified which columns were being searched (PatientId and DateTime), I created an index on those columns."

### Q2: "What's the difference between a table scan and an index seek?"
**A:** "A table scan reads every row in the table and checks if it matches the condition. An index seek uses the index structure to jump directly to matching rows. In this case, the index seek reduced from 50+ logical reads to 2-3, meaning we're accessing far fewer pages."

### Q3: "Why did you use a composite index instead of separate indexes?"
**A:** "A composite index on (PatientId, DateTime DESC) is more efficient because the data is pre-sorted by PatientId, then by DateTime. Separate indexes would require additional merge steps. The composite index lets SQL find rows matching both conditions in a single structure."

### Q4: "Tell me about one challenging bug."
**A:** "The null reference exception was tricky because it only occurred under specific conditions—when a technician was deleted. I had to understand the relationship between PatientAssessment and AspNetUsers, then implement eager loading to ensure the related data was always loaded. I also added null-safety checks throughout the code."

### Q5: "How would you handle a high-volume scenario?"
**A:** "Several ways: First, the indexes would make queries faster. Second, I could add caching for frequently accessed patient summaries. Third, I could implement pagination to avoid loading too much data at once. Finally, I could add database connection pooling in the application."

### Q6: "What would you do differently if you had more time?"
**A:** "I would add caching for aggregate queries like health summaries. I would implement logging and monitoring to track slow queries. I would add more comprehensive unit tests for edge cases. I would also implement API rate limiting to prevent abuse."

### Q7: "How do you test your optimizations?"
**A:** "I use SQL Server's execution plan comparison tool to compare BEFORE and AFTER plans. I measure query execution time using SET STATISTICS TIME ON. I count logical reads with SET STATISTICS IO ON. All these metrics prove the optimization worked."

### Q8: "Describe your development process."
**A:** "I started by understanding the existing database schema and analyzing which queries were slow. I used execution plans to identify missing indexes. I created the indexes and tested them locally. Then I built the API endpoints to use these optimized queries. Finally, I added validation and error handling for robustness."

---

## Live Demo Script (5 minutes)

### Demo: Query Optimization
```
1. Open SSMS
2. Run slow query:
   SELECT * FROM CDSSResult 
   WHERE DateTime >= '2024-01-01' AND DateTime <= '2024-12-31'
   
3. Show: Execution Plan (Table Scan)
4. Show: Properties (450ms, 50 logical reads)

5. Create index:
   CREATE NONCLUSTERED INDEX IX_CDSSResult_PatientId_DateTime
   ON CDSSResult (PatientId, DateTime DESC)
   INCLUDE (CAT_TotalScore, MRC_Scale, HealthScore)

6. Run same query again
7. Show: Execution Plan (Index Seek)
8. Show: Properties (12ms, 2-3 logical reads)

9. Explain: "37x faster, 94% fewer reads"
```

### Demo: Running the API
```
1. Open Visual Studio
2. Show project structure (Models, Controllers, DbContext)
3. Run the application (dotnet run)
4. Open Swagger UI at https://localhost:5001/swagger
5. Test endpoint: GET /api/assessments/2?startDate=2024-01-01&endDate=2024-12-31
6. Show response with patient assessments
7. Test endpoint: GET /api/patients/2/health-summary
8. Show aggregated health data
```

### Demo: Error Handling
```
1. Try to create assessment with invalid health score:
   POST /api/patients/2/ckd
   Body: { "healthScore": 150 }
   
2. Show: 400 Bad Request with error message
3. Show: "HealthScore must be 0-100"
4. Try valid request
5. Show: 201 Created with successful record
```

---

## What NOT to Say

❌ "I don't remember exactly what I did"  
❌ "I just copied code from Stack Overflow"  
❌ "The code was already there"  
❌ "I didn't understand the performance issue"  
❌ "I didn't test my changes"  
❌ "The bugs were minor"  
❌ Avoid over-technical jargon without explanation

---

## What TO Say

✅ "I analyzed execution plans to identify bottlenecks"  
✅ "I measured performance before and after"  
✅ "I validated my changes with testing"  
✅ "I implemented validation at multiple layers"  
✅ "I focused on data quality and reliability"  
✅ "I considered trade-offs (e.g., index maintenance cost)"  
✅ Explain concepts clearly to a non-specialist

---

## Practice Tips

1. **Read these talking points aloud 3x** — builds confidence and natural flow
2. **Practice in front of a mirror** — observe your body language
3. **Record yourself** — listen for clarity and pacing
4. **Time yourself** — each topic should fit the allocated time
5. **Prepare demo in advance** — test all commands before the actual viva
6. **Have notes visible** — don't memorize, but know the structure
7. **Anticipate questions** — review Q&A section and practice answers

