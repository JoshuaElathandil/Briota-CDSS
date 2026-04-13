using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Briota.CDSS.Models
{
    public class PatientInfo
    {
        [Key]
        public int Id { get; set; }

        public string? UniqueId { get; set; }
        
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }

        // Gender is INT in this database
        public int Gender { get; set; } = 1; // Default to Male or other non-null value
        
        public string? Email { get; set; }
        public string? ContactNumber { get; set; }
        
        public DateTime DateOfBirth { get; set; } = DateTime.Now;
        
        // Use float for 'real' columns
        public float? Height { get; set; }
        public float? Weight { get; set; }
        
        // AreYouSmoker is BIT
        public bool AreYouSmoker { get; set; } = false;
        
        public string? Symptoms { get; set; }
        public string? TriggerAndAllergies { get; set; }
        
        // Medical metrics are NVARCHAR in this specific table
        public string? FVC { get; set; }
        public string? FEVI { get; set; }
        public string? PEF { get; set; }

        public int? CenterId { get; set; }
        public int? DistrictId { get; set; }
        public int? SubCenterId { get; set; }

        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

        public float? SystollicBloodPressure { get; set; }
        public float? DistolicBloodPressure { get; set; }
        
        public string? MedicalHistory { get; set; }
        public bool? IsActive { get; set; }

        [ConcurrencyCheck]
        [MaxLength(450)]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
