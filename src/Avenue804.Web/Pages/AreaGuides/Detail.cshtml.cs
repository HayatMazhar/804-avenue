using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.AreaGuides;

public class AreaGuideDetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public AreaGuideDetailModel(ApplicationDbContext db) => _db = db;
    public AreaGuide? Guide { get; private set; }
    public List<PropertyListing> AreaListings { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct = default)
    {
        Guide = await _db.AreaGuides.AsNoTracking()
            .FirstOrDefaultAsync(g => g.IsPublished && g.Slug == slug.Trim(), ct);
        if (Guide == null) return NotFound();

        ViewData["NavActive"] = "communities";
        ViewData["Title"] = Guide.Name + " — Community Guide";
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(Guide.ShortDescription)
            ? Guide.ShortDescription
            : $"Explore {Guide.Name}{(string.IsNullOrWhiteSpace(Guide.Emirate) ? string.Empty : $" in {Guide.Emirate}")} — amenities, lifestyle, schools, transport and average returns.";

        AreaListings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Location != null && p.Location.Contains(Guide.Name))
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(6)
            .ToListAsync(ct);

        return Page();
    }
}
