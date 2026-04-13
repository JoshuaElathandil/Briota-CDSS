using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientPortalApp.Data;
using System;
using System.Linq;

namespace PatientPortalApp.Pages
{
    public class ReportModel : PageModel
    {
        private readonly AppDbContext _context;

        public ReportModel(AppDbContext context)
        {
            _context = context;
        }

        public string PatientName { get; set; } = string.Empty;
        public DateTime DateOfEntry { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string Medication { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public IActionResult OnGet(int? patientId)
        {
            if (patientId == null)
            {
                return NotFound();
            }

            var patient = _context.Patients.FirstOrDefault(p => p.Id == patientId.Value);
            if (patient == null)
            {
                return NotFound();
            }

            PatientName = patient.Name;
            DateOfEntry = patient.DateOfEntry;

            var parts = patient.ReportText.Split(
                new[] { "\\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);

            var notesBuilder = new System.Text.StringBuilder();

            foreach (var part in parts)
            {
                var line = part.Trim();

                if (line.StartsWith("Diagnosis:", StringComparison.OrdinalIgnoreCase))
                {
                    Diagnosis = line.Substring("Diagnosis:".Length).Trim();
                }
                else if (line.StartsWith("Medication:", StringComparison.OrdinalIgnoreCase))
                {
                    Medication = line.Substring("Medication:".Length).Trim();
                }
                else if (line.StartsWith("Prescription:", StringComparison.OrdinalIgnoreCase))
                {
                    Medication = line.Substring("Prescription:".Length).Trim();
                }
                else if (line.StartsWith("Treatment:", StringComparison.OrdinalIgnoreCase))
                {
                    Medication = line.Substring("Treatment:".Length).Trim();
                }
                else if (line.StartsWith("Notes:", StringComparison.OrdinalIgnoreCase))
                {
                    Notes = line.Substring("Notes:".Length).Trim();
                }
                else
                {
                    if (notesBuilder.Length > 0)
                        notesBuilder.AppendLine();

                    notesBuilder.Append(line);
                }
            }

            if (string.IsNullOrWhiteSpace(Notes) && notesBuilder.Length > 0)
            {
                Notes = notesBuilder.ToString();
            }
            else if (notesBuilder.Length > 0)
            {
                Notes = string.IsNullOrWhiteSpace(Notes)
                    ? notesBuilder.ToString()
                    : Notes + Environment.NewLine + notesBuilder;
            }

            return Page();
        }
    }
}