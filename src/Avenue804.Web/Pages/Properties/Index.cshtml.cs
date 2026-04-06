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

    public async Task OnGetAsync(string? offer, CancellationToken cancellationToken = default)
    {
        ViewData["NavActive"] = "properties";
        Filter = offer;
        var q = _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "");

        if (string.Equals(offer, "rent", StringComparison.OrdinalIgnoreCase))
            q = q.Where(p => p.OfferType == ListingOfferType.Rent);
        else if (string.Equals(offer, "sale", StringComparison.OrdinalIgnoreCase))
            q = q.Where(p => p.OfferType == ListingOfferType.Sale);

        Listings = await q
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
