using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Briota.CDSS.Models
{
    public class CDSSResult
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public DateTime DateTime { get; set; }

        // COPD Assessment Test (CAT)
        [Range(0, 40)]
        public int CAT_TotalScore { get; set; }
        
        [Range(0, 5)]
        public int CAT_Cough { get; set; }
        
        [Range(0, 5)]
        public int CAT_Phlegm { get; set; }
        
        [Range(0, 5)]
        public int CAT_ChestTight { get; set; }
        
        [Range(0, 5)]
        public int CAT_HillStairs { get; set; }
        
        [Range(0, 5)]
        public int CAT_LimitedActivity { get; set; }
        
        [Range(0, 5)]
        public int CAT_ConfLeavingHome { get; set; }
        
        [Range(0, 5)]
        public int CAT_SleepSoundly { get; set; }
        
        [Range(0, 5)]
        public int CAT_LotsOfEnergy { get; set; }

        // MMRC Scale
        [Range(0, 4)]
        public int MRC_Scale { get; set; }

        public int Exacerbations_History { get; set; }
        
        [MaxLength(10)]
        public string? CDSSZone { get; set; } // A, B, C, D
        
        [MaxLength(50)]
        public string? TestGrade { get; set; }

        public string? TechnicianId { get; set; }
        
        [ForeignKey("PatientId")]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual ApplicationUser? Technician { get; set; }

        [ConcurrencyCheck]
        [MaxLength(450)]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
