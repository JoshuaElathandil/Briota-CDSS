# Bug Fixes & Error Handling

## Bug #1: Null Reference Exception on TechnicianId

### Symptom
App crashes with `NullReferenceException` when loading `PatientAssessment` if technician has been deleted or reassigned.

### Root Cause
- Foreign key `TechnicianId` in `PatientAssessment` allows NULL
- Code accesses `Technician.FirstName` without checking if relationship loaded
- No eager loading with `.Include()`

### Bad Code (Current)
```csharp
public IActionResult GetAssessment(int id)
{
  var assessment = db.PatientAssessments.Find(id);
  
  // CRASH HERE if assessment.Technician is null
  var technicianName = assessment.Technician.FirstName;
  
  return Ok(new { assessment, technicianName });
}
```

### Fixed Code
```csharp
public async Task<IActionResult> GetAssessment(int id)
{
  // FIX 1: Eager load the Technician relationship
  var assessment = await db.PatientAssessments
    .Include(p => p.Technician)
    .FirstOrDefaultAsync(p => p.Id == id);
  
  // FIX 2: Check if assessment exists
  if (assessment == null)
    return NotFound($"Assessment {id} not found");
  
  // FIX 3: Use null-coalescing operator for missing technician
  var technicianName = assessment.Technician?.FirstName ?? "Unassigned";
  
  return Ok(new { assessment, technicianName });
}
```

### What to Say in Viva
*"The system crashed when retrieving assessments if the technician relationship was missing. I fixed this by using eager loading with .Include() to fetch the Technician in the same query, and then used the null-coalescing operator (??) to provide a default value if the technician was NULL. This prevents both the null reference exception and provides a better user experience."*

---

## Bug #2: Invalid Health Score Values

### Symptom
HealthScore in CKDDetails sometimes shows -50, 150, or other values outside the valid 0-100 range.

### Root Cause
- No validation at database level (column is just `int`)
- No validation in C# code before saving
- Possible manual SQL inserts without constraints
- No constraint on the column definition

### Bad Code (Current)
```csharp
public IActionResult CreateCKDAssessment(CreateCKDDto dto)
{
  // NO VALIDATION!
  var ckd = new CKDDetails
  {
    PatientId = dto.PatientId,
    HealthScore = dto.HealthScore,  // Could be -50 or 150
    RiskLevel = dto.RiskLevel,
    UpdatedDate = DateTime.UtcNow
  };
  
  db.CKDDetails.Add(ckd);  // SAVES INVALID DATA!
  db.SaveChanges();
  
  return Ok(ckd);
}
```

### Fixed Code (Option 1: API Validation)
```csharp
public async Task<IActionResult> CreateCKDAssessment([FromBody] CreateCKDDto dto)
{
  // FIX 1: Validate health score
  if (dto.HealthScore < 0 || dto.HealthScore > 100)
  {
    return BadRequest(new 
    { 
      error = "HealthScore must be between 0 and 100",
      received = dto.HealthScore
    });
  }
  
  // FIX 2: Validate risk level
  if (dto.RiskLevel < 0 || dto.RiskLevel > 5)
  {
    return BadRequest("RiskLevel must be between 0 and 5");
  }
  
  // FIX 3: Validate patient exists
  var patient = await db.PatientInfos.FindAsync(dto.PatientId);
  if (patient == null)
    return NotFound($"Patient {dto.PatientId} not found");
  
  var ckd = new CKDDetails
  {
    PatientId = dto.PatientId,
    HealthScore = dto.HealthScore,
    RiskLevel = dto.RiskLevel,
    UpdatedDate = DateTime.UtcNow
  };
  
  db.CKDDetails.Add(ckd);
  await db.SaveChangesAsync();
  
  return CreatedAtAction(nameof(GetCKDAssessment), new { id = ckd.Id }, ckd);
}
```

### Fixed Code (Option 2: Data Annotation Validation)
```csharp
public class CKDDetails
{
  public int Id { get; set; }
  
  [Range(0, 100, ErrorMessage = "HealthScore must be between 0 and 100")]
  public int HealthScore { get; set; }
  
  [Range(0, 5, ErrorMessage = "RiskLevel must be between 0 and 5")]
  public int RiskLevel { get; set; }
  
  [Required]
  public int PatientId { get; set; }
  
  public PatientInfo Patient { get; set; }
}
```

### Fixed Code (Option 3: Database Constraint)
```sql
-- Add CHECK constraint in SQL Server
ALTER TABLE CKDDetails
ADD CONSTRAINT CHK_HealthScore_Range 
CHECK (HealthScore >= 0 AND HealthScore <= 100);

ALTER TABLE CKDDetails
ADD CONSTRAINT CHK_RiskLevel_Range 
CHECK (RiskLevel >= 0 AND RiskLevel <= 5);
```

### What to Say in Viva
*"Health scores were being saved with invalid values because there was no validation. I implemented validation at multiple levels: first at the API level using data annotations, then in the controller with explicit range checks, and finally added a database constraint. This ensures that invalid data can never be inserted, either through the API or direct SQL."*

---

## Bug #3: Orphaned Assessment Records

### Symptom
Assessment records exist in the database with `PatientAssessmentId` pointing to non-existent patients or deleted assessments.

### Root Cause
- Foreign key constraint is not enforced at application level
- No validation before inserting Assessment records
- Possible manual data entry errors
- Race condition between checking and inserting

### Bad Code (Current)
```csharp
public IActionResult CreateAssessment(int patientAssessmentId, CreateAssessmentDto dto)
{
  // NO CHECK: Does PatientAssessment even exist?
  var assessment = new Assessment
  {
    PatientAssessmentId = patientAssessmentId,
    AssessmentQuestionId = dto.QuestionId,
    Answer = dto.Answer
  };
  
  db.Assessments.Add(assessment);  // SAVES ORPHANED RECORD!
  db.SaveChanges();
  
  return Ok(assessment);
}
```

### Fixed Code
```csharp
public async Task<IActionResult> CreateAssessment(
  int patientAssessmentId, 
  [FromBody] CreateAssessmentDto dto)
{
  // FIX 1: Verify PatientAssessment exists
  var patientAssessment = await db.PatientAssessments
    .FindAsync(patientAssessmentId);
  
  if (patientAssessment == null)
    return NotFound($"PatientAssessment {patientAssessmentId} not found");
  
  // FIX 2: Verify AssessmentQuestion exists
  var question = await db.AssessmentQuestions
    .FindAsync(dto.QuestionId);
  
  if (question == null)
    return NotFound($"AssessmentQuestion {dto.QuestionId} not found");
  
  // FIX 3: Validate answer is not empty
  if (string.IsNullOrWhiteSpace(dto.Answer))
    return BadRequest("Answer cannot be empty");
  
  // FIX 4: Only create assessment if all references are valid
  var assessment = new Assessment
  {
    PatientAssessmentId = patientAssessmentId,
    AssessmentQuestionId = dto.QuestionId,
    Answer = dto.Answer.Trim()
  };
  
  db.Assessments.Add(assessment);
  await db.SaveChangesAsync();
  
  return CreatedAtAction(nameof(GetAssessment), new { id = assessment.Id }, assessment);
}
```

### Alternative: Use Transactions for Safety
```csharp
public async Task<IActionResult> CreateAssessmentWithTransaction(
  int patientAssessmentId, 
  [FromBody] CreateAssessmentDto dto)
{
  using (var transaction = db.Database.BeginTransaction())
  {
    try
    {
      // All of this succeeds or all rolls back
      var patientAssessment = await db.PatientAssessments
        .FindAsync(patientAssessmentId);
      if (patientAssessment == null) throw new InvalidOperationException("PatientAssessment not found");
      
      var question = await db.AssessmentQuestions
        .FindAsync(dto.QuestionId);
      if (question == null) throw new InvalidOperationException("Question not found");
      
      var assessment = new Assessment
      {
        PatientAssessmentId = patientAssessmentId,
        AssessmentQuestionId = dto.QuestionId,
        Answer = dto.Answer
      };
      
      db.Assessments.Add(assessment);
      await db.SaveChangesAsync();
      
      await transaction.CommitAsync();
      
      return CreatedAtAction(nameof(GetAssessment), new { id = assessment.Id }, assessment);
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();
      return BadRequest($"Failed to create assessment: {ex.Message}");
    }
  }
}
```

### What to Say in Viva
*"Assessments could be created with invalid patient assessment references because there was no validation at the application level. I fixed this by checking that both the PatientAssessment and AssessmentQuestion exist before allowing the record to be created. I also added transaction handling so that if any validation fails, the entire operation rolls back and no orphaned records are created."*

---

## Summary of All Bug Fixes

| Bug | Root Cause | Fix | Impact |
|-----|-----------|-----|--------|
| Null Reference | No eager loading | .Include() + null coalescing | Prevents crashes |
| Invalid Health Score | No validation | Range check + data annotations | Data quality |
| Orphaned Records | No referential checks | Existence validation + transaction | Referential integrity |

---

## Testing the Fixes

### Test Case 1: Null Reference
```csharp
[Fact]
public async Task GetAssessment_WhenTechnicianDeleted_ReturnsUnassigned()
{
  var assessment = new PatientAssessment { TechnicianId = null };
  db.PatientAssessments.Add(assessment);
  await db.SaveChangesAsync();
  
  var result = await controller.GetAssessment(assessment.Id);
  
  var okResult = Assert.IsType<OkObjectResult>(result);
  Assert.Contains("Unassigned", okResult.Value.ToString());
}
```

### Test Case 2: Invalid Health Score
```csharp
[Fact]
public async Task CreateCKD_WhenHealthScoreTooHigh_ReturnsBadRequest()
{
  var dto = new CreateCKDDto { HealthScore = 150 };
  
  var result = await controller.CreateCKDAssessment(dto);
  
  var badRequest = Assert.IsType<BadRequestObjectResult>(result);
  Assert.Contains("must be between 0 and 100", badRequest.Value.ToString());
}
```

### Test Case 3: Orphaned Record
```csharp
[Fact]
public async Task CreateAssessment_WhenPatientAssessmentMissing_ReturnsNotFound()
{
  var dto = new CreateAssessmentDto { QuestionId = 1, Answer = "Yes" };
  
  var result = await controller.CreateAssessment(9999, dto); // Invalid ID
  
  Assert.IsType<NotFoundObjectResult>(result);
}
```

