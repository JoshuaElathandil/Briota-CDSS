using Microsoft.AspNetCore.Mvc.RazorPages;
using PatientPortalApp.Data;
using PatientPortalApp.Models;
using System.Collections.Generic;
using System.Linq;

namespace PatientPortalApp.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly AppDbContext _context;

        public DashboardModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Patient> Patients { get; set; } = new();

        public void OnGet()
        {
            Patients = _context.Patients
                .OrderBy(p => p.Id)
                .ToList();
        }
    }
}