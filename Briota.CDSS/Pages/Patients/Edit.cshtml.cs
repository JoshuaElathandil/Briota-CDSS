using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;

namespace Briota.CDSS.Pages.Patients
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class EditModel : PageModel
    {
        private readonly HealthDbContext _context;

        public EditModel(HealthDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PatientInfo Patient { get; set; } = default!;

        [BindProperty]
        public string? DbSymptoms { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patients.FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }
            Patient = patient;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var patientToUpdate = await _context.Patients.FirstOrDefaultAsync(p => p.Id == Patient.Id);

            if (patientToUpdate == null)
            {
                return NotFound();
            }

            // DR. QUINN: Update only the symptoms for this lab demo
            patientToUpdate.Symptoms = Patient.Symptoms;

            // MANDATORY: Rotate the stamp every time to ensure deterministic concurrency failure
            patientToUpdate.ConcurrencyStamp = Guid.NewGuid().ToString();

            // Update the concurrency stamp from the form (the original one) to trigger the check
            _context.Entry(patientToUpdate).Property("ConcurrencyStamp").OriginalValue = Patient.ConcurrencyStamp;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // DR. QUINN: AHA! The conflict is detected here.
                var entry = ex.Entries.Single();
                var databaseEntry = entry.GetDatabaseValues();
                
                if (databaseEntry == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to save changes. The patient was deleted by another user.");
                }
                else
                {
                    var databaseValues = (PatientInfo)databaseEntry.ToObject();

                    if (databaseValues.Symptoms != Patient.Symptoms)
                    {
                        ModelState.AddModelError("Patient.Symptoms", $"Current database value: {databaseValues.Symptoms}");
                    }
                    
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user after you fetched it. The current database values are shown below. If you still want to edit, click Save again.");
                    
                    Patient.ConcurrencyStamp = databaseValues.ConcurrencyStamp;
                    DbSymptoms = databaseValues.Symptoms;
                    ModelState.Remove("Patient.ConcurrencyStamp");
                }
                
                return Page();
            }

            return RedirectToPage("./Index");
        }
    }
}
