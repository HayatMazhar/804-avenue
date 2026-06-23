using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Developers;

public class DevelopersIndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DevelopersIndexModel(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Active developers (those with a slug — without a slug the public
    /// profile page cannot be linked).
    /// </summary>
    public IReadOnlyList<DeveloperRow> Developers { get; private set; } = Array.Empty<DeveloperRow>();

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["NavActive"] = "developers";

        // Pull active developers + a count of their published listings in one query
        // to avoid N+1 lookups on the index page.
        Developers = await _db.Developers
            .AsNoTracking()
            .Where(d => d.IsActive && d.Slug != null && d.Slug != "")
            .OrderBy(d => d.Name)
            .Select(d => new DeveloperRow
            {
                Id = d.Id,
                Name = d.Name,
                Slug = d.Slug!,
                LogoUrl = d.LogoUrl,
                Headquarters = d.Headquarters,
                EstablishedYear = d.EstablishedYear,
                Description = d.Description,
                ListingCount = _db.PropertyListings.Count(p => p.IsPublished && p.DeveloperId == d.Id),
                OffPlanCount = _db.PropertyListings.Count(p => p.IsPublished && p.IsOffPlan && p.DeveloperId == d.Id),
            })
            .ToListAsync(ct);
    }

    public class DeveloperRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? LogoUrl { get; set; }
        public string? Headquarters { get; set; }
        public int? EstablishedYear { get; set; }
        public string? Description { get; set; }
        public int ListingCount { get; set; }
        public int OffPlanCount { get; set; }
    }
}
