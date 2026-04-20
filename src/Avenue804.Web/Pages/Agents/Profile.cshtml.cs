using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Agents;

public class AgentProfileModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public AgentProfileModel(ApplicationDbContext db) => _db = db;
    public Agent? Agent { get; private set; }
    public List<PropertyListing> Listings { get; private set; } = [];
    public int ListingCount { get; private set; }
    public int TotalViews { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct = default)
    {
        Agent = await _db.Agents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.IsActive, ct);
        if (Agent == null) return NotFound();
        ViewData["Title"] = Agent.Name + " — Property Agent";

        Listings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.AgentId == id)
            .OrderByDescending(p => p.ViewCount)
            .ToListAsync(ct);

        ListingCount = Listings.Count;
        TotalViews = Listings.Sum(p => p.ViewCount);
        return Page();
    }
}
