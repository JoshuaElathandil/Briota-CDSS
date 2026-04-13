using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Briota.CDSS.Models
{
    public class Assessment
    {
        [Key]
        public int Id { get; set; }

        public int PatientAssessmentId { get; set; }
        public int AssessmentQuestionId { get; set; }

        public string? Answer { get; set; }

        [ForeignKey("PatientAssessmentId")]
        public virtual PatientAssessment? PatientAssessment { get; set; }

        [ForeignKey("AssessmentQuestionId")]
        public virtual AssessmentQuestion? Question { get; set; }
    }

    public class AssessmentQuestion
    {
        [Key]
        public int Id { get; set; }

        public string? Question { get; set; }
    }
}
