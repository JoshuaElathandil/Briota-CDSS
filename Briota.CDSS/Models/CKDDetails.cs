using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Briota.CDSS.Models
{
    public class CKDDetails
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }

        // Core Vitals
        [Range(0, 500)]
        public double ACRValue { get; set; }
        
        [Range(40, 250)]
        public float SystollicBloodPressure { get; set; }
        
        [Range(30, 150)]
        public float DistolicBloodPressure { get; set; }
        
        [Range(50, 250)]
        public float Height { get; set; }
        
        [Range(20, 300)]
        public float Weight { get; set; }

        // AI Results
        [Range(0, 100)]
        public int HealthScore { get; set; }
        
        [Range(0, 5)]
        public int RiskLevel { get; set; }
        
        public string? PredictedLabel { get; set; }
        public float? PredictedScore { get; set; }

        // Symptoms (Bit flags)
        public bool HasNauseaVomitingLossOfAppetite { get; set; }
        public bool HasSleepingProblem { get; set; }
        public bool UrinatingMoreOrLess { get; set; }
        public bool MuscleCrampSwellingOfFeetAndAnkelDryOrItchySkin { get; set; }
        public bool DoesItBurnWhileUrinating { get; set; }

        // Metadata
        public string? Diagnosis { get; set; }
        public string? DoctorNotes { get; set; }
        public DateTime? DiagnosisDate { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        
        [ForeignKey("CreatedBy")]
        public virtual ApplicationUser? Creator { get; set; }
        
        public int? LocationDetailsId { get; set; }

        public string? PDFReportLink { get; set; }
        public string? ReportLink { get; set; }
        public string? Comment { get; set; }
        public string? FindingsSubmittedBy { get; set; }
        public DateTime? FinidngSubmittedDate { get; set; } // Matching DB typo

        [ConcurrencyCheck]
        [MaxLength(450)]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
