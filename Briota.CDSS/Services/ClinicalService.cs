using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;
using System.Security.Claims;

namespace Briota.CDSS.Services
{
    public interface ICenterService
    {
        int? GetCurrentCenterId();
    }

    public class CenterService : ICenterService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CenterService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetCurrentCenterId()
        {
            var centerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("CenterId")?.Value;
            if (int.TryParse(centerIdClaim, out int centerId))
            {
                return centerId;
            }
            return null;
        }
    }

    public interface IClinicalService
    {
        Task<List<CKDDetails>> GetPatientsCkdHistoryAsync(int patientId);
        Task<List<CDSSResult>> GetPatientsCopdHistoryAsync(int patientId);
        Task<PatientAssessment?> GetAssessmentDetailsAsync(int id);
    }

    public class ClinicalService : IClinicalService
    {
        private readonly HealthDbContext _context;

        public ClinicalService(HealthDbContext context)
        {
            _context = context;
        }

        public async Task<List<CKDDetails>> GetPatientsCkdHistoryAsync(int patientId)
        {
            return await _context.CKDDetails
                .Include(c => c.Creator)
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<CDSSResult>> GetPatientsCopdHistoryAsync(int patientId)
        {
            return await _context.CDSSResults
                .Include(c => c.Technician)
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.DateTime)
                .ToListAsync();
        }

        public async Task<PatientAssessment?> GetAssessmentDetailsAsync(int id)
        {
            return await _context.PatientAssessments
                .Include(p => p.Technician)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
