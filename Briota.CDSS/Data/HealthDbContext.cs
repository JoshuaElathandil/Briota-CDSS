using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Models;
using Briota.CDSS.Services;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Briota.CDSS.Data
{
    public class HealthDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly int? _currentCenterId;

        public HealthDbContext(DbContextOptions<HealthDbContext> options, ICenterService centerService)
            : base(options)
        {
            _currentCenterId = centerService.GetCurrentCenterId();
        }

        public DbSet<CDSSResult> CDSSResults { get; set; }
        public DbSet<CKDDetails> CKDDetails { get; set; }
        public DbSet<PatientInfo> Patients { get; set; }
        public DbSet<PatientAssessment> PatientAssessments { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentQuestion> AssessmentQuestions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Identity Specific Mapping
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(u => u.Id).HasMaxLength(900);
                entity.Property(u => u.UniqueId).HasMaxLength(900);
                
                // Fix for SqlException 544: Identity column handling
                // entity.Property(u => u.EmployeeNumber).ValueGeneratedOnAdd().Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

                // Security: Filter identity users by center
                entity.HasQueryFilter(u => !_currentCenterId.HasValue || u.CenterId == _currentCenterId);
            });

            // 2. Clinical Tables with Security Isolation
            builder.Entity<CKDDetails>(entity =>
            {
                entity.ToTable("CKDDetails");
                entity.Property(e => e.HealthScore).HasDefaultValue(0);
                entity.Property(e => e.PredictedScore).HasColumnType("real");
                entity.Property(e => e.ACRValue).HasColumnType("float");
                
                // Security: Filter by CenterId if available
                entity.HasQueryFilter(e => !_currentCenterId.HasValue || e.LocationDetailsId == _currentCenterId); 
            });

            builder.Entity<PatientInfo>(entity =>
            {
                entity.ToTable("PatientInfo");
                entity.HasQueryFilter(p => !_currentCenterId.HasValue || p.CenterId == _currentCenterId);
            });

            builder.Entity<CDSSResult>(entity =>
            {
                entity.ToTable("CDSSResult");
                entity.HasQueryFilter(e => !_currentCenterId.HasValue || (e.Patient != null && e.Patient.CenterId == _currentCenterId));
            });

            builder.Entity<PatientAssessment>(entity =>
            {
                entity.ToTable("PatientAssessment");
                entity.HasQueryFilter(e => !_currentCenterId.HasValue || (e.Patient != null && e.Patient.CenterId == _currentCenterId));
            });

            builder.Entity<Assessment>(entity =>
            {
                entity.ToTable("Assessment");
                entity.HasQueryFilter(e => !_currentCenterId.HasValue || (e.PatientAssessment != null && e.PatientAssessment.Patient != null && e.PatientAssessment.Patient.CenterId == _currentCenterId));
            });

            builder.Entity<AssessmentQuestion>(entity =>
            {
                entity.ToTable("AssessmentQuestion");
            });
        }
    }
}
