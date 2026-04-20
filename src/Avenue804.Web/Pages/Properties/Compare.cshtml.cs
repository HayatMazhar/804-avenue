using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Properties;

public class CompareModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CompareModel(ApplicationDbContext db) => _db = db;

    public List<PropertyListing> Listings { get; private set; } = [];

    public async Task OnGetAsync(string? ids, CancellationToken ct = default)
    {
        ViewData["NavActive"] = "properties";

        if (string.IsNullOrWhiteSpace(ids)) return;

        var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(s => int.TryParse(s, out var n) ? n : 0)
            .Where(n => n > 0)
            .Take(3)
            .ToList();

        if (idList.Count == 0) return;

        Listings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && idList.Contains(p.Id))
            .ToListAsync(ct);
    }
}
