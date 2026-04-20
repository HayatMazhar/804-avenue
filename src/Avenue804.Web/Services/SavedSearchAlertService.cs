using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Services;

/// <summary>Checks all active saved searches and emails users whose criteria match a newly published listing.</summary>
public class SavedSearchAlertService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IFeatureFlagService _flags;
    private readonly IConfiguration _cfg;
    private readonly ILogger<SavedSearchAlertService> _log;

    public SavedSearchAlertService(
        ApplicationDbContext db, IEmailSender email, IFeatureFlagService flags,
        IConfiguration cfg, ILogger<SavedSearchAlertService> log)
    {
        _db = db; _email = email; _flags = flags; _cfg = cfg; _log = log;
    }

    public async Task NotifyMatchingUsersAsync(PropertyListing listing, string baseUrl, CancellationToken ct = default)
    {
        if (!await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, ct)) return;

        var searches = await _db.SavedSearches
            .Include(s => s.User)
            .Where(s => s.AlertEnabled && s.User.Email != null && s.User.IsPublicUser)
            .ToListAsync(ct);

        int notified = 0;
        foreach (var search in searches)
        {
            if (!Matches(listing, search)) continue;

            var url = $"{baseUrl}/Properties/{listing.Slug}";
            await _email.SendAsync(search.User.Email!,
                $"New property matches your saved search — {listing.Title}",
                $"""
                <h3>A new listing matches your saved search: <em>{search.Label ?? "Saved search"}</em></h3>
                <p><strong>{listing.Title}</strong></p>
                <p>{(listing.Location != null ? "📍 " + listing.Location : "")}</p>
                <p>Price: {(listing.Price.HasValue ? $"AED {listing.Price.Value:N0}" : "Price on request")}</p>
                <p><a href="{url}" style="display:inline-block;padding:12px 24px;background:#F59E0B;color:#fff;border-radius:8px;text-decoration:none;font-weight:700">View Property</a></p>
                <hr/>
                <p style="font-size:12px;color:#94a3b8">You're receiving this because you saved a search on 804 Avenue.
                <a href="{baseUrl}/Account/Dashboard">Manage your saved searches</a>.</p>
                """, ct);

            search.LastAlertedAt = DateTimeOffset.UtcNow;
            notified++;
        }

        if (notified > 0)
        {
            await _db.SaveChangesAsync(ct);
            _log.LogInformation("Sent saved-search alerts to {Count} user(s) for listing {Slug}.", notified, listing.Slug);
        }
    }

    private static bool Matches(PropertyListing listing, SavedSearch search)
    {
        // Offer type
        if (!string.IsNullOrWhiteSpace(search.Offer))
        {
            if (search.Offer == "sale" && listing.OfferType != ListingOfferType.Sale) return false;
            if (search.Offer == "rent" && listing.OfferType != ListingOfferType.Rent) return false;
        }
        // Location
        if (!string.IsNullOrWhiteSpace(search.Location) && !string.IsNullOrWhiteSpace(listing.Location))
            if (!listing.Location.Contains(search.Location, StringComparison.OrdinalIgnoreCase)) return false;
        // Keyword
        if (!string.IsNullOrWhiteSpace(search.Keyword))
        {
            var kw = search.Keyword;
            if (!listing.Title.Contains(kw, StringComparison.OrdinalIgnoreCase) &&
                (listing.Location == null || !listing.Location.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                return false;
        }
        // Budget
        if (!string.IsNullOrWhiteSpace(search.Budget) && listing.Price.HasValue)
        {
            var p = listing.Price.Value;
            if (search.Budget == "u500"  && p >= 500_000) return false;
            if (search.Budget == "500-1m" && (p < 500_000 || p >= 1_000_000)) return false;
            if (search.Budget == "1m-5m"  && (p < 1_000_000 || p >= 5_000_000)) return false;
            if (search.Budget == "5m+"   && p < 5_000_000) return false;
        }
        return true;
    }
}
