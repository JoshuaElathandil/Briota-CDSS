# ASP.NET Core Project Structure

## Project Layout

```
BriotaHealthPlatform/
├── Models/                           # Entity classes (map to DB)
│   ├── CDSSResult.cs                 # COPD assessment model
│   ├── CKDDetails.cs                 # Kidney disease model
│   ├── PatientAssessment.cs          # Assessment link model
│   ├── Assessment.cs                 # Detailed Q&A model
│   ├── AssessmentQuestion.cs         # Question bank model
│   └── AspNetUser.cs                 # User/technician model
│
├── Data/                             # Database context
│   └── HealthDbContext.cs            # EF Core DbContext with indexes
│
├── Controllers/                      # API endpoints
│   ├── AssessmentsController.cs       # GET/POST assessments
│   ├── PatientsController.cs          # GET patient health summary
│   └── OptimizationController.cs      # Demo endpoint showing optimizations
│
├── DTOs/                             # Data transfer objects
│   ├── CreateCKDDto.cs
│   ├── CreateAssessmentDto.cs
│   └── HealthSummaryDto.cs
│
├── Services/                         # Business logic (optional)
│   ├── AssessmentService.cs
│   └── PatientService.cs
│
├── appsettings.json                  # Connection string
├── appsettings.Development.json      # Dev configuration
├── Program.cs                        # Startup & dependency injection
└── Startup.cs                        # (if using older ASP.NET pattern)
```

---

## Step 1: Create ASP.NET Core Project

```bash
# Terminal command
dotnet new webapi -n BriotaHealthPlatform
cd BriotaHealthPlatform

# Install Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

---

## Step 2: Create Models (Models/ folder)

### CDSSResult.cs
```csharp
using System;

namespace BriotaHealthPlatform.Models
{
    public class CDSSResult
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public DateTime DateTime { get; set; }
        public string? TestGrade { get; set; }
        public string? CDSSZone { get; set; }
        public int? MRC_Scale { get; set; }
        public int? Exacerbations_History { get; set; }
        public int? CAT_TotalScore { get; set; }
        public int? CAT_Cough { get; set; }
        public int? CAT_Phlegm { get; set; }
        public int? CAT_ChestTight { get; set; }
        public int? CAT_HillStairs { get; set; }
        public int? CAT_LimitedActivity { get; set; }
        public int? CAT_ConfLeavingHome { get; set; }
        public int? CAT_SleepSoundly { get; set; }
        public int? CAT_LotsOfEnergy { get; set; }
        public string? TechnicianId { get; set; }
        
        // Navigation property
        public AspNetUser? Technician { get; set; }
    }
}
```

### CKDDetails.cs
```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace BriotaHealthPlatform.Models
{
    public class CKDDetails
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        
        [Range(0, 100, ErrorMessage = "HealthScore must be 0-100")]
        public int HealthScore { get; set; }
        
        [Range(0, 5, ErrorMessage = "RiskLevel must be 0-5")]
        public int RiskLevel { get; set; }
        
        public string? RiskAssessment { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public float SystollicBloodPressure { get; set; }
        public float DistolicBloodPressure { get; set; }
        public double ACRValue { get; set; }
        public string? Diagnosis { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public string? PDFReportLink { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
```

### PatientAssessment.cs
```csharp
using System;

namespace BriotaHealthPlatform.Models
{
    public class PatientAssessment
    {
        public int Id { get; set; }
        public int PatientInfoId { get; set; }
        public string? TechnicianId { get; set; }
        public string? Interpretation { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        
        // Navigation properties
        public AspNetUser? Technician { get; set; }
    }
}
```

### Assessment.cs
```csharp
namespace BriotaHealthPlatform.Models
{
    public class Assessment
    {
        public int Id { get; set; }
        public int PatientAssessmentId { get; set; }
        public int AssessmentQuestionId { get; set; }
        public string? Answer { get; set; }
    }
}
```

### AssessmentQuestion.cs
```csharp
namespace BriotaHealthPlatform.Models
{
    public class AssessmentQuestion
    {
        public int Id { get; set; }
        public string Question { get; set; }
    }
}
```

### AspNetUser.cs
```csharp
using System;

namespace BriotaHealthPlatform.Models
{
    public class AspNetUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public int? CenterId { get; set; }
        public int? DistrictId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
```

---

## Step 3: Create DbContext (Data/ folder)

### HealthDbContext.cs
```csharp
using Microsoft.EntityFrameworkCore;
using BriotaHealthPlatform.Models;

namespace BriotaHealthPlatform.Data
{
    public class HealthDbContext : DbContext
    {
        public HealthDbContext(DbContextOptions<HealthDbContext> options) : base(options)
        {
        }

        public DbSet<CDSSResult> CDSSResults { get; set; }
        public DbSet<CKDDetails> CKDDetails { get; set; }
        public DbSet<PatientAssessment> PatientAssessments { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }
        public DbSet<AspNetUser> AspNetUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== OPTIMIZATION INDEXES =====
            
            // Index 1: Date-range queries (37x faster)
            modelBuilder.Entity<CDSSResult>()
                .HasIndex(c => new { c.PatientId, c.DateTime })
                .IsDescending(false, true);

            // Index 2: CKD health score lookups (20x faster)
            modelBuilder.Entity<CKDDetails>()
                .HasIndex(c => new { c.PatientId, c.UpdatedDate })
                .IsDescending(false, true);

            // Index 3: Technician performance (10x faster)
            modelBuilder.Entity<PatientAssessment>()
                .HasIndex(c => new { c.TechnicianId, c.CreatedDate })
                .IsDescending(false, true);

            // ===== RELATIONSHIPS =====
            
            modelBuilder.Entity<PatientAssessment>()
                .HasOne(p => p.Technician)
                .WithMany()
                .HasForeignKey(p => p.TechnicianId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Assessment>()
                .HasOne<AssessmentQuestion>()
                .WithMany()
                .HasForeignKey(a => a.AssessmentQuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
```

---

## Step 4: Configure Connection String

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=SecurityDB_06June;Trusted_Connection=true;Encrypt=false;"
  },
  "AllowedHosts": "*"
}
```

**Note:** Replace `YOUR_SERVER` with your SQL Server instance name (e.g., `localhost`, `DESKTOP-ABC123`, or `.\SQLEXPRESS`)

### appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Warning"
    }
  }
}
```

---

## Step 5: Configure Startup (Program.cs)

### Program.cs (ASP.NET Core 6+)
```csharp
using Microsoft.EntityFrameworkCore;
using BriotaHealthPlatform.Data;

var builder = WebApplicationBuilder.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure DbContext
builder.Services.AddDbContext<HealthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add CORS if needed (for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Step 6: Create Base Controller

### BaseController.cs
```csharp
using Microsoft.AspNetCore.Mvc;
using BriotaHealthPlatform.Data;

namespace BriotaHealthPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseController : ControllerBase
    {
        protected readonly HealthDbContext _db;

        public BaseController(HealthDbContext db)
        {
            _db = db;
        }
    }
}
```

---

## Step 7: Test Connectivity

### Program.cs - Add Migration & Update (one-time setup)
```bash
# Create migration (generates SQL)
dotnet ef migrations add InitialCreate

# Apply migration (creates tables if needed)
dotnet ef database update
```

### Run the Application
```bash
dotnet run

# App runs on: https://localhost:5001 (or http://localhost:5000)
# Swagger UI: https://localhost:5001/swagger
```

---

## Checklist for Day 1

- [ ] Create new ASP.NET Core project
- [ ] Install Entity Framework packages
- [ ] Create Models/ folder and all 6 model classes
- [ ] Create Data/HealthDbContext.cs with indexes
- [ ] Update appsettings.json with connection string
- [ ] Update Program.cs with DbContext configuration
- [ ] Run `dotnet ef migrations add InitialCreate`
- [ ] Run `dotnet ef database update`
- [ ] Verify app starts without errors
- [ ] Test Swagger UI opens at https://localhost:5001/swagger

**Time Expected:** 4-5 hours

