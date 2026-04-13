using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace Briota.CDSS.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TechnicalShowroomModel : PageModel
    {
        private readonly HealthDbContext _context;

        public TechnicalShowroomModel(HealthDbContext context)
        {
            _context = context;
        }

        public string SearchResult { get; set; } = "";
        public long SearchLatency { get; set; }
        public string ConcurrencyResult { get; set; } = "";
        public string SafetyResult { get; set; } = "";
        public bool ShowBaseline { get; set; }

        public void OnGet()
        {
        }

        // --- SEARCH PILLAR ---

        public async Task<IActionResult> OnPostRunSlowSearchAsync(string query)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            // TIER 0 BASELINE SIMULATOR: 
            // 1. Non-SARGable query (Contains)
            // 2. Artificial latency to simulate lack of indexing on large datasets
            await Task.Delay(2500); 
            var result = await _context.Patients
                .Where(p => p.FirstName!.Contains(query) || p.LastName!.Contains(query))
                .Take(1)
                .FirstOrDefaultAsync();
            
            sw.Stop();
            SearchLatency = sw.ElapsedMilliseconds;
            SearchResult = result != null ? $"Match Found: {result.FirstName} {result.LastName}" : "No match found.";
            ShowBaseline = true;
            return Page();
        }

        public async Task<IActionResult> OnPostRunFastSearchAsync(string query)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            // TIER 1 PRODUCTION LOGIC:
            // SARGable query (StartsWith) utilizing SQL Indexes
            var result = await _context.Patients
                .Where(p => p.FirstName!.StartsWith(query) || p.LastName!.StartsWith(query))
                .Take(1)
                .FirstOrDefaultAsync();
            
            sw.Stop();
            SearchLatency = sw.ElapsedMilliseconds;
            SearchResult = result != null ? $"Match Found: {result.FirstName} {result.LastName}" : "No match found.";
            ShowBaseline = false;
            return Page();
        }

        // --- CONCURRENCY PILLAR ---

        public async Task<IActionResult> OnPostSimulateOverwriteAsync()
        {
            // TIER 0 BASELINE: Silent Overwrite (No check)
            var p = await _context.Patients.FirstAsync();
            p.Symptoms = "SYMPTOM_OVERWRITTEN_" + DateTime.Now.Ticks % 1000;
            
            // We purposefully DON'T rotate or check the stamp
            await _context.SaveChangesAsync();
            
            ConcurrencyResult = "SUCCESS (Silent Overwrite): The previous data was lost without warning.";
            return Page();
        }

        public async Task<IActionResult> OnPostSimulateConflictAsync()
        {
            // TIER 1 PRODUCTION: Optimistic Locking
            var p = await _context.Patients.FirstAsync();
            p.Symptoms = "CONFLICT_TEST";
            
            // Force a stamp mismatch by setting the original value incorrectly in the entry
            _context.Entry(p).Property("ConcurrencyStamp").OriginalValue = "WRONG_STAMP";
            
            try
            {
                await _context.SaveChangesAsync();
                ConcurrencyResult = "Save Succeeded";
            }
            catch (DbUpdateConcurrencyException)
            {
                ConcurrencyResult = "BLOCK DETECTED (OCC Alert): Clinical version conflict caught and blocked.";
            }
            
            return Page();
        }

        // --- RESILIENCE PILLAR ---

        public IActionResult OnPostSimulateCrash()
        {
            // TIER 0 BASELINE: Implicit Trust Crash
            try 
            {
                PatientInfo? p = null; 
                var name = p!.FirstName; // Forced NullReferenceException
                SafetyResult = name ?? "Fault Simulated";
            }
            catch (Exception ex)
            {
                SafetyResult = $"SYSTEM CRASH (500): {ex.GetType().Name} - Application halted.";
            }
            return Page();
        }

        public IActionResult OnPostSimulateSafe()
        {
            // TIER 1 PRODUCTION: Handle missing relations gracefully
            PatientInfo? p = null;
            var name = p?.FirstName ?? "Unassigned (Resilient Fallback)";
            SafetyResult = $"STABLE: {name}";
            return Page();
        }
    }
}
