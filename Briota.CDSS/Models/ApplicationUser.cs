using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Briota.CDSS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string? UniqueId { get; set; }
        
        public int? CenterId { get; set; }
        
        public bool IsActive { get; set; }

        public string? RegistrationNumber { get; set; }
        
        public string? ClinicRegistrationNumber { get; set; }
        
        public int? DistrictId { get; set; }
        public int? StateId { get; set; }

        // public int EmployeeNumber { get; set; } // Removing to avoid identity insert errors in seeding
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
