using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages;

public class MarketTrendsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public MarketTrendsModel(ApplicationDbContext db) => _db = db;

    public int TotalListings { get; private set; }
    public int ForSaleCount { get; private set; }
    public int ForRentCount { get; private set; }
    public int OffPlanCount { get; private set; }
    public decimal? AvgSalePrice { get; private set; }
    public decimal? AvgRentPrice { get; private set; }
    public double? AvgYield { get; private set; }
    public int TotalViews { get; private set; }
    public List<PropertyListing> MostViewed { get; private set; } = [];
    public List<(string location, int count)> TopLocations { get; private set; } = [];
    public List<object> PriceByLocation { get; private set; } = [];
    public List<object> ListingsByType { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var listings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished).ToListAsync(ct);

        TotalListings = listings.Count;
        ForSaleCount = listings.Count(p => p.OfferType == ListingOfferType.Sale);
        ForRentCount = listings.Count(p => p.OfferType == ListingOfferType.Rent);
        OffPlanCount = listings.Count(p => p.IsOffPlan);
        TotalViews = listings.Sum(p => p.ViewCount);

        var sales = listings.Where(p => p.OfferType == ListingOfferType.Sale && p.Price.HasValue).ToList();
        var rents = listings.Where(p => p.OfferType == ListingOfferType.Rent && p.Price.HasValue).ToList();

        AvgSalePrice = sales.Any() ? sales.Average(p => p.Price!.Value) : null;
        AvgRentPrice = rents.Any() ? rents.Average(p => p.Price!.Value) : null;

        if (AvgSalePrice.HasValue && AvgRentPrice.HasValue && AvgSalePrice.Value > 0)
            AvgYield = (double)(AvgRentPrice.Value / AvgSalePrice.Value * 100);

        MostViewed = listings.OrderByDescending(p => p.ViewCount).Take(10).ToList();

        // Top locations
        TopLocations = listings
            .Where(p => !string.IsNullOrWhiteSpace(p.Location))
            .GroupBy(p => p.Location!.Split(',')[0].Trim())
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => (g.Key, g.Count()))
            .ToList();

        // Price by location (top 8 sale listings with prices)
        PriceByLocation = sales
            .Where(p => !string.IsNullOrWhiteSpace(p.Location))
            .GroupBy(p => p.Location!.Split(',')[0].Trim())
            .Select(g => new { location = g.Key, avgPrice = (long)g.Average(p => p.Price!.Value), count = g.Count() })
            .OrderByDescending(x => x.avgPrice)
            .Take(8)
            .Cast<object>()
            .ToList();

        ListingsByType = [ForSaleCount, ForRentCount];
    }
}
