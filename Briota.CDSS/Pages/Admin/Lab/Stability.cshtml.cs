using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;
using Microsoft.AspNetCore.Authorization;

namespace Briota.CDSS.Pages.Admin.Lab
{
    [Authorize(Roles = "Admin")]
    public class StabilityModel : PageModel
    {
        private readonly HealthDbContext _context;

        public StabilityModel(HealthDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Mode { get; set; } = "Tier1";

        public PatientInfo? SamplePatient { get; set; }
        public string ErrorMessage { get; set; } = "";
        public bool DidCrash { get; set; }

        public void OnGet()
        {
            // We simulate a clinical case where the primary entity is found but a related field is null
            // For this demo, we'll just set SamplePatient to null to trigger the "Access Fault"
            SamplePatient = null; 

            if (Mode == "Tier0")
            {
                // TIER 0: Baseline Prototype (Implicit Trust)
                // Accessing properties directly without null checks.
                try
                {
                    // This is the actual code pattern that caused crashes in the baseline
                    var name = SamplePatient!.FirstName; 
                }
                catch (NullReferenceException)
                {
                    DidCrash = true;
                    ErrorMessage = "Unhandled NullReferenceException: Object reference not set to an instance of an object.";
                }
            }
            else
            {
                // TIER 1: Production Hardened
                // Using null-coalescing and safe-access
                var name = SamplePatient?.FirstName ?? "Unassigned Clinic Record";
                DidCrash = false;
            }
        }
    }
}
