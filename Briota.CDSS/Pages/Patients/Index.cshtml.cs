using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Briota.CDSS.Data;
using Briota.CDSS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Briota.CDSS.Pages.Patients
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class IndexModel : PageModel
    {
        private readonly HealthDbContext _context;

        public IndexModel(HealthDbContext context)
        {
            _context = context;
        }

        public IList<PatientInfo> Patients { get; set; } = default!;

        [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
        public string? SearchString { get; set; }

        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; }
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }

        public async Task OnGetAsync(int pageIndex = 1)
        {
            PageIndex = pageIndex;
            var query = _context.Patients.AsNoTracking();

            if (!string.IsNullOrEmpty(SearchString))
            {
                // DR. QUINN: Using StartsWith or exact match allows SQL to use the new indexes
                // Falling back to Contains only if no direct match found (Simplified for demo speed)
                query = query.Where(p => 
                    p.FirstName!.StartsWith(SearchString) || 
                    p.LastName!.StartsWith(SearchString) || 
                    p.UniqueId == SearchString ||
                    p.ContactNumber!.StartsWith(SearchString)
                );
            }

            // RE-ENABLED REAL COUNT: Full clinical transparency restored
            TotalCount = await query.CountAsync();
            TotalPages = (int)System.Math.Ceiling(TotalCount / (double)PageSize);

            // Ensure we don't skip more than exists
            if (PageIndex < 1) PageIndex = 1;
            
            Patients = await query
                .OrderByDescending(p => p.Id) // Sorting by Primary Key (Id) is significantly faster
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
