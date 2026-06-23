using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.AreaGuides;

public class AreaGuidesIndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public AreaGuidesIndexModel(ApplicationDbContext db) => _db = db;

    public List<AreaGuide> Guides { get; private set; } = [];
    public Dictionary<string, List<AreaGuide>> Grouped { get; private set; } = [];
    public Dictionary<int, int> ListingCounts { get; private set; } = [];
    public List<string> AvailableEmirates { get; private set; } = [];
    public int TotalCount { get; private set; }

    [BindProperty(SupportsGet = true, Name = "q")] public string? Keyword { get; set; }
    [BindProperty(SupportsGet = true, Name = "emirate")] public string? EmirateFilter { get; set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["NavActive"] = "communities";

        var allPublished = await _db.AreaGuides.AsNoTracking()
            .Where(g => g.IsPublished)
            .OrderBy(g => g.Emirate).ThenBy(g => g.Name)
            .ToListAsync(ct);

        TotalCount = allPublished.Count;

        AvailableEmirates = allPublished
            .Where(g => !string.IsNullOrWhiteSpace(g.Emirate))
            .Select(g => g.Emirate!)
            .Distinct()
            .OrderBy(e => e)
            .ToList();

        IEnumerable<AreaGuide> filtered = allPublished;
        if (!string.IsNullOrWhiteSpace(EmirateFilter))
            filtered = filtered.Where(g => string.Equals(g.Emirate, EmirateFilter, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            var k = Keyword.Trim();
            filtered = filtered.Where(g =>
                g.Name.Contains(k, StringComparison.OrdinalIgnoreCase)
                || (g.City != null && g.City.Contains(k, StringComparison.OrdinalIgnoreCase))
                || (g.ShortDescription != null && g.ShortDescription.Contains(k, StringComparison.OrdinalIgnoreCase))
                || (g.PopularWith != null && g.PopularWith.Contains(k, StringComparison.OrdinalIgnoreCase)));
        }

        Guides = filtered.ToList();

        // Approximate "listings nearby" per community — used as a small stat chip on each card.
        var names = Guides.Select(g => g.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count > 0)
        {
            ListingCounts = new Dictionary<int, int>();
            foreach (var g in Guides)
            {
                var count = await _db.PropertyListings.AsNoTracking()
                    .CountAsync(p => p.IsPublished && p.Location != null && p.Location.Contains(g.Name), ct);
                ListingCounts[g.Id] = count;
            }
        }

        Grouped = Guides.GroupBy(g => g.Emirate ?? "Other")
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
