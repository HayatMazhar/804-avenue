using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Properties;

public class OffPlanModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public OffPlanModel(ApplicationDbContext db) => _db = db;

    public IReadOnlyList<PropertyListing> Listings { get; private set; } = [];
    public int TotalCount { get; private set; }
    public string? Keyword { get; private set; }

    public async Task OnGetAsync(string? q, string? location, string? handover, string? budget, CancellationToken ct = default)
    {
        ViewData["NavActive"] = "properties";
        Keyword = q;

        var query = _db.PropertyListings.AsNoTracking()
            .Include(p => p.Developer)
            .Where(p => p.IsPublished && p.IsOffPlan && p.Slug != null);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Title.Contains(q) || (p.Location != null && p.Location.Contains(q)));

        if (!string.IsNullOrWhiteSpace(location))
            query = query.Where(p => p.Location != null && p.Location.Contains(location));

        if (!string.IsNullOrWhiteSpace(handover))
            query = query.Where(p => p.HandoverDate != null && p.HandoverDate.Contains(handover));

        if (!string.IsNullOrWhiteSpace(budget))
        {
            query = budget switch
            {
                "u500" => query.Where(p => p.Price < 500_000),
                "500-1m" => query.Where(p => p.Price >= 500_000 && p.Price < 1_000_000),
                "1m-5m" => query.Where(p => p.Price >= 1_000_000 && p.Price < 5_000_000),
                "5m+" => query.Where(p => p.Price >= 5_000_000),
                _ => query
            };
        }

        TotalCount = await query.CountAsync(ct);
        Listings = await query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt).ToListAsync(ct);
    }
}
