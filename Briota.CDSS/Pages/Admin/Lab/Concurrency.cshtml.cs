using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;
using Microsoft.AspNetCore.Authorization;

namespace Briota.CDSS.Pages.Admin.Lab
{
    [Authorize(Roles = "Admin")]
    public class ConcurrencyModel : PageModel
    {
        private readonly HealthDbContext _context;

        public ConcurrencyModel(HealthDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string TargetSymptoms { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string Mode { get; set; } = "Tier1"; 

        [BindProperty]
        public string ProvidedStampA { get; set; } = "";

        [BindProperty]
        public string ProvidedStampB { get; set; } = "";

        public string Status { get; set; } = "";
        public bool IsConflict { get; set; }
        public PatientInfo Patient { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Patient = await _context.Patients.FirstAsync();
            TargetSymptoms = Patient.Symptoms ?? "";
            
            // On fresh load or mode switch, synchronize both terminals
            ProvidedStampA = Patient.ConcurrencyStamp;
            ProvidedStampB = Patient.ConcurrencyStamp;
            
            Status = $"Environment: {Mode} | Both Terminals Synchronized.";
        }

        public async Task<IActionResult> OnPostSaveAsync(string terminal)
        {
            var p = await _context.Patients.FirstAsync();
            
            // Use the specific stamp from the acting terminal context
            string activeStamp = (terminal == "A") ? ProvidedStampA : ProvidedStampB;

            if (Mode == "Tier1")
            {
                // TIER 1: Production Hardened - Enforce Check
                _context.Entry(p).Property("ConcurrencyStamp").OriginalValue = activeStamp;
                
                // Only modify symptoms if we are successfully tracking the version
                p.Symptoms = TargetSymptoms;

                // Explicitly rotate the stamp on success
                p.ConcurrencyStamp = Guid.NewGuid().ToString();
            }
            else
            {
                // TIER 0: Baseline Prototype - Bypass Check
                p.Symptoms = TargetSymptoms;
                _context.Entry(p).Property("ConcurrencyStamp").IsModified = false;
            }

            try
            {
                await _context.SaveChangesAsync();
                Status = $"TRANSACTION SUCCESS: [Terminal {terminal}] pushed a new version.";
                IsConflict = false;
            }
            catch (DbUpdateConcurrencyException)
            {
                Status = $"CRITICAL CONFLICT: [Terminal {terminal}] is using a STALE version. Update blocked to prevent clinical data loss.";
                IsConflict = true;
            }

            // CRITICAL: Clear ModelState to allow the UI to refresh with the updated 'Model' properties
            // Without this, Tag Helpers would prefer the 'stale' values from the POST request.
            var previousStampA = ProvidedStampA;
            var previousStampB = ProvidedStampB;
            ModelState.Clear();

            // Reload the definitive record from the database
            Patient = await _context.Patients.FirstAsync();
            
            // --- STRICT ISOLATION REFRESH ---
            if (terminal == "A" && !IsConflict)
            {
                ProvidedStampA = Patient.ConcurrencyStamp; // Refresh only A
                ProvidedStampB = previousStampB;          // Keep B stale for the demo
            }
            else if (terminal == "B" && !IsConflict)
            {
                ProvidedStampB = Patient.ConcurrencyStamp; // Refresh only B
                ProvidedStampA = previousStampA;          // Keep A stale
            }
            else
            {
                // On failure or other states, restore the stamps from the POST to maintain the 'stale' visual
                ProvidedStampA = previousStampA;
                ProvidedStampB = previousStampB;
            }

            return Page();
        }
    }
}
