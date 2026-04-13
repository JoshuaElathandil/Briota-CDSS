using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;
using Briota.CDSS.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Briota.CDSS.Pages.Patients
{
    public class DetailsModel : PageModel
    {
        private readonly HealthDbContext _context;
        private readonly IClinicalService _clinicalService;

        public DetailsModel(HealthDbContext context, IClinicalService clinicalService)
        {
            _context = context;
            _clinicalService = clinicalService;
        }

        public PatientInfo? Patient { get; set; }
        public List<CKDDetails> CkdHistory { get; set; } = new();
        public List<CDSSResult> CopdHistory { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Patient = await _context.Patients.FirstOrDefaultAsync(p => p.Id == id);

            if (Patient == null)
            {
                return NotFound();
            }

            CkdHistory = await _clinicalService.GetPatientsCkdHistoryAsync(id);
            CopdHistory = await _clinicalService.GetPatientsCopdHistoryAsync(id);

            return Page();
        }
    }
}
