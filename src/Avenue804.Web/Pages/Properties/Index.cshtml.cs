using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Properties;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public IReadOnlyList<PropertyListing> Listings { get; private set; } = [];
    public string? Filter { get; private set; }
    public string? SearchLocation { get; private set; }
    public string? SearchArea { get; private set; }
    public string? SearchCategory { get; private set; }
    public string? SearchType { get; private set; }
    public string? SearchSize { get; private set; }
    public string? SearchBudget { get; private set; }
    public string? SearchKeyword { get; private set; }
    public string? SortBy { get; private set; }
    public int TotalCount { get; private set; }

    public async Task OnGetAsync(
        string? offer, string? location, string? area, string? category, string? type, string? size, string? budget, string? q,
        string? sort,
        CancellationToken cancellationToken = default)
    {
        ViewData["NavActive"] = "properties";
        Filter = offer; SearchLocation = location; SearchArea = area; SearchCategory = category;
        SearchType = type; SearchSize = size; SearchBudget = budget; SearchKeyword = q; SortBy = sort ?? "newest";

        var query = _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "");

        if (string.Equals(offer, "rent", StringComparison.OrdinalIgnoreCase))
            query = query.Where(p => p.OfferType == ListingOfferType.Rent);
        else if (string.Equals(offer, "sale", StringComparison.OrdinalIgnoreCase))
            query = query.Where(p => p.OfferType == ListingOfferType.Sale);

        if (!string.IsNullOrWhiteSpace(location) && location != "any")
        {
            query = query.Where(p =>
                (p.Emirates != null && p.Emirates == location) ||
                (p.Location != null && p.Location.Contains(location)));
        }

        if (!string.IsNullOrWhiteSpace(area))
            query = query.Where(p => p.Location != null && p.Location.Contains(area));

        if (!string.IsNullOrWhiteSpace(category) && category != "any")
            query = query.Where(p => p.PropertyCategory != null && p.PropertyCategory == category);

        if (!string.IsNullOrWhiteSpace(type) && type != "any")
        {
            query = query.Where(p =>
                (p.PropertyType != null && p.PropertyType == type) ||
                p.Title.Contains(type) ||
                (p.Description != null && p.Description.Contains(type)));
        }

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Title.Contains(q) || (p.Location != null && p.Location.Contains(q)) || (p.Description != null && p.Description.Contains(q)) || (p.PropertyType != null && p.PropertyType.Contains(q)));

        // Size band — operates against AreaSqft column.
        if (!string.IsNullOrWhiteSpace(size) && size != "any")
        {
            query = size switch
            {
                "u500"      => query.Where(p => p.AreaSqft != null && p.AreaSqft < 500),
                "500-1000"  => query.Where(p => p.AreaSqft != null && p.AreaSqft >= 500 && p.AreaSqft < 1000),
                "1000-2000" => query.Where(p => p.AreaSqft != null && p.AreaSqft >= 1000 && p.AreaSqft < 2000),
                "2000-5000" => query.Where(p => p.AreaSqft != null && p.AreaSqft >= 2000 && p.AreaSqft < 5000),
                "5000+"     => query.Where(p => p.AreaSqft != null && p.AreaSqft >= 5000),
                _ => query
            };
        }

        if (!string.IsNullOrWhiteSpace(budget) && budget != "any")
        {
            query = budget switch
            {
                "u500"  => query.Where(p => p.Price < 500_000),
                "500-1m"=> query.Where(p => p.Price >= 500_000 && p.Price < 1_000_000),
                "1m-5m" => query.Where(p => p.Price >= 1_000_000 && p.Price < 5_000_000),
                "5m+"   => query.Where(p => p.Price >= 5_000_000),
                _ => query
            };
        }

        TotalCount = await query.CountAsync(cancellationToken);

        query = sort switch
        {
            "price-asc"  => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "views"      => query.OrderByDescending(p => p.ViewCount),
            "reduced"    => query.Where(p => p.PreviousPrice.HasValue && p.Price < p.PreviousPrice)
                                 .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt),
            "verified"   => query.Where(p => p.IsVerified).OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt),
            _            => query.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
        };

        Listings = await query.ToListAsync(cancellationToken);
    }
}
