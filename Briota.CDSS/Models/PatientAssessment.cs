using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Briota.CDSS.Models
{
    public class PatientAssessment
    {
        [Key]
        public int Id { get; set; }

        public int PatientInfoId { get; set; }

        public string? TechnicianId { get; set; }

        public string? Interpretation { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        [ForeignKey("PatientInfoId")]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey("TechnicianId")]
        public virtual ApplicationUser? Technician { get; set; }

        [ConcurrencyCheck]
        [MaxLength(450)]
        public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
