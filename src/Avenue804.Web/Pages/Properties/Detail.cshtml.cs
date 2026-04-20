using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Properties;

public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DetailModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public PropertyListing? Listing { get; private set; }
    public bool IsSaved { get; private set; }
    public double AverageRating { get; private set; }
    public int RatingCount { get; private set; }
    public List<PropertyRating> ApprovedRatings { get; private set; } = [];
    public PropertyRating? UserRating { get; private set; }
    public List<PropertyListing> SimilarListings { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct = default)
    {
        ViewData["NavActive"] = "properties";
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        var key = slug.Trim();
        Listing = await _db.PropertyListings.AsNoTracking()
            .Include(p => p.Agent)
            .Include(p => p.Developer)
            .FirstOrDefaultAsync(p => p.IsPublished && p.Slug == key, ct);
        if (Listing == null) return NotFound();

        // Increment view count atomically (ExecuteUpdate avoids concurrency issues)
        await _db.PropertyListings
            .Where(p => p.Id == Listing.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1), ct);

        // Use SEO overrides if set by admin, otherwise auto-generate
        ViewData["Title"] = !string.IsNullOrWhiteSpace(Listing.SeoTitle)
            ? Listing.SeoTitle
            : Listing.Title;

        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(Listing.SeoDescription)
            ? Listing.SeoDescription
            : string.IsNullOrWhiteSpace(Listing.Description)
                ? $"{Listing.Title} — 804 Avenue Properties, {Listing.Location ?? "Abu Dhabi UAE"}"
                : Listing.Description.Length > 160
                    ? Listing.Description[..160].Trim() + "…"
                    : Listing.Description;
        ViewData["OgImage"] = Listing.MainImageUrl;

        // Ratings
        var ratings = await _db.PropertyRatings
            .Include(r => r.User)
            .Where(r => r.ListingId == Listing.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        ApprovedRatings = ratings.Where(r => r.IsApproved).ToList();
        RatingCount = ApprovedRatings.Count;
        AverageRating = RatingCount > 0 ? ApprovedRatings.Average(r => r.Stars) : 0;

        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _users.GetUserAsync(User);
            if (user != null)
            {
                UserRating = ratings.FirstOrDefault(r => r.UserId == user.Id);
                IsSaved = await _db.SavedProperties.AnyAsync(s => s.UserId == user.Id && s.ListingId == Listing.Id, ct);
            }
        }

        // Similar properties — same type + close price range
        var loc = Listing.Location?.Split(',')[0] ?? "";
        SimilarListings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Id != Listing.Id && p.OfferType == Listing.OfferType
                && (loc == "" || (p.Location != null && p.Location.Contains(loc))))
            .OrderByDescending(p => p.ViewCount)
            .Take(3)
            .ToListAsync(ct);

        // Fallback: any published listings if no location match
        if (SimilarListings.Count == 0)
        {
            SimilarListings = await _db.PropertyListings.AsNoTracking()
                .Where(p => p.IsPublished && p.Id != Listing.Id && p.OfferType == Listing.OfferType)
                .OrderByDescending(p => p.ViewCount)
                .Take(3)
                .ToListAsync(ct);
        }

        return Page();
    }
}
