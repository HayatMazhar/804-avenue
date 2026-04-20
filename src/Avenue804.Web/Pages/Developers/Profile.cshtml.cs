using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Developers;

public class DeveloperProfileModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeveloperProfileModel(ApplicationDbContext db) => _db = db;
    public Developer? Developer { get; private set; }
    public List<PropertyListing> Listings { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct = default)
    {
        Developer = await _db.Developers.AsNoTracking()
            .FirstOrDefaultAsync(d => d.IsActive && d.Slug == slug.Trim(), ct);
        if (Developer == null) return NotFound();

        Listings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.DeveloperId == Developer.Id)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .ToListAsync(ct);

        return Page();
    }
}
