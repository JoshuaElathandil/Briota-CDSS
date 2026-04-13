using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Briota.CDSS.Pages
{
    public class TestDbModel : PageModel
    {
        private readonly HealthDbContext _context;

        public TestDbModel(HealthDbContext context)
        {
            _context = context;
        }

        public bool CanConnect { get; set; }
        public int PatientCount { get; set; }
        public int CkdCount { get; set; }
        public int CopdCount { get; set; }
        public List<string>? Roles { get; set; }

        public async Task OnGetAsync()
        {
            CanConnect = await _context.Database.CanConnectAsync();
            
            if (CanConnect)
            {
                // Standard counts using AsNoTracking for speed
                PatientCount = await _context.Patients.AsNoTracking().IgnoreQueryFilters().CountAsync();
                CkdCount = await _context.CKDDetails.AsNoTracking().IgnoreQueryFilters().CountAsync();
                CopdCount = await _context.CDSSResults.AsNoTracking().IgnoreQueryFilters().CountAsync();
                
                Roles = await _context.Roles.Select(r => r.Name!).ToListAsync();
            }
        }
    }
}
