using System.Text.Json;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Models.Admin;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.Extensions;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminAccess")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;
    private readonly SavedSearchAlertService _alerts;
    private readonly IHomeStatsService _homeStats;

    public EditModel(ApplicationDbContext db, IEmailSender email, IConfiguration cfg, IFeatureFlagService flags, SavedSearchAlertService alerts, IHomeStatsService homeStats)
    {
        _db = db; _email = email; _cfg = cfg; _flags = flags; _alerts = alerts; _homeStats = homeStats;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public PropertyListingForm Form { get; set; } = new();

    public List<Developer> Developers { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        var entity = await _db.PropertyListings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null) return NotFound();
        Form = MapFrom(entity);
        Developers = await _db.Developers.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        Developers = await _db.Developers.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync(cancellationToken);
        var entity = await _db.PropertyListings.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null) return NotFound();
        if (!ModelState.IsValid) return Page();

        var baseSlug = string.IsNullOrWhiteSpace(Form.Slug)
            ? SlugGenerator.FromTitle(Form.Title)
            : SlugGenerator.FromTitle(Form.Slug.Trim(), Form.Title);
        var slug = await UniqueSlug.ForPropertyListingAsync(_db, baseSlug, Id, cancellationToken);

        // Track price history
        var priceChanged = entity.Price.HasValue && Form.Price.HasValue && Form.Price != entity.Price;
        var priceDropped = priceChanged && Form.Price < entity.Price;
        var oldPrice = entity.Price;

        if (priceChanged && oldPrice.HasValue)
        {
            // Append to price history JSON
            var history = new System.Text.Json.Nodes.JsonArray();
            if (!string.IsNullOrWhiteSpace(entity.PriceHistoryJson))
            {
                try { history = System.Text.Json.Nodes.JsonNode.Parse(entity.PriceHistoryJson)?.AsArray() ?? new(); }
                catch { }
            }
            history.Add(System.Text.Json.Nodes.JsonNode.Parse(
                $"{{\"date\":\"{DateTimeOffset.UtcNow:yyyy-MM-dd}\",\"price\":{(long)oldPrice.Value}}}"));
            entity.PriceHistoryJson = history.ToJsonString();
        }

        entity.Title = Form.Title.Trim();
        entity.Slug = slug;
        if (priceDropped) entity.PreviousPrice = oldPrice;
        entity.Price = Form.Price;
        entity.Currency = string.IsNullOrWhiteSpace(Form.Currency) ? "AED" : Form.Currency.Trim();
        entity.OfferType = Form.OfferType;
        entity.Location = string.IsNullOrWhiteSpace(Form.Location) ? null : Form.Location.Trim();
        entity.PropertyCategory = string.IsNullOrWhiteSpace(Form.PropertyCategory) ? null : Form.PropertyCategory.Trim();
        entity.PropertyType = string.IsNullOrWhiteSpace(Form.PropertyType) ? null : Form.PropertyType.Trim();
        entity.Emirates = string.IsNullOrWhiteSpace(Form.Emirates) ? null : Form.Emirates.Trim();
        entity.Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim();
        entity.Beds = Form.Beds;
        entity.Baths = Form.Baths;
        entity.AreaSqft = Form.AreaSqft;
        entity.MainImageUrl = string.IsNullOrWhiteSpace(Form.MainImageUrl) ? null : Form.MainImageUrl.Trim();
        entity.GalleryImagesJson = ParseGalleryUrls(Form.GalleryImagesJson);
        entity.FloorPlanUrl = string.IsNullOrWhiteSpace(Form.FloorPlanUrl) ? null : Form.FloorPlanUrl.Trim();
        entity.VirtualTourUrl = string.IsNullOrWhiteSpace(Form.VirtualTourUrl) ? null : Form.VirtualTourUrl.Trim();
        entity.Label = string.IsNullOrWhiteSpace(Form.Label) ? null : Form.Label.Trim();
        entity.AmenitiesJson = string.IsNullOrWhiteSpace(Form.AmenitiesJson) ? null : Form.AmenitiesJson.Trim();
        entity.IsOffPlan = Form.IsOffPlan;
        entity.HandoverDate = Form.IsOffPlan ? Form.HandoverDate?.Trim() : null;
        entity.PaymentPlan = Form.IsOffPlan ? Form.PaymentPlan?.Trim() : null;
        entity.CompletionPercent = Form.IsOffPlan ? Form.CompletionPercent : null;

        // ProjectStatus + Subtitle + ProjectAddress remain available for non-off-plan listings too
        // (they're harmless empty for regular listings) — but we treat them as part of the off-plan profile.
        entity.Subtitle = string.IsNullOrWhiteSpace(Form.Subtitle) ? null : Form.Subtitle.Trim();
        entity.ProjectStatus = string.IsNullOrWhiteSpace(Form.ProjectStatus) ? null : Form.ProjectStatus.Trim();
        entity.ProjectAddress = string.IsNullOrWhiteSpace(Form.ProjectAddress) ? null : Form.ProjectAddress.Trim();

        entity.UnitTypes = Form.IsOffPlan ? (string.IsNullOrWhiteSpace(Form.UnitTypes) ? null : Form.UnitTypes.Trim()) : null;
        entity.BedroomOptions = Form.IsOffPlan ? (string.IsNullOrWhiteSpace(Form.BedroomOptions) ? null : Form.BedroomOptions.Trim()) : null;
        entity.BathroomOptions = Form.IsOffPlan ? (string.IsNullOrWhiteSpace(Form.BathroomOptions) ? null : Form.BathroomOptions.Trim()) : null;
        entity.StartingSizeSqft = Form.IsOffPlan ? Form.StartingSizeSqft : null;
        entity.TotalFloors = Form.IsOffPlan ? Form.TotalFloors : null;
        entity.TotalBuildings = Form.IsOffPlan ? Form.TotalBuildings : null;
        entity.TotalUnits = Form.IsOffPlan ? Form.TotalUnits : null;

        entity.DownPaymentPercent = Form.IsOffPlan ? Form.DownPaymentPercent : null;
        entity.DuringConstructionPercent = Form.IsOffPlan ? Form.DuringConstructionPercent : null;
        entity.OnHandoverPercent = Form.IsOffPlan ? Form.OnHandoverPercent : null;

        entity.KeyFeatures = Form.IsOffPlan ? (string.IsNullOrWhiteSpace(Form.KeyFeatures) ? null : Form.KeyFeatures.Trim()) : null;
        entity.AmenitiesDescription = Form.IsOffPlan ? (string.IsNullOrWhiteSpace(Form.AmenitiesDescription) ? null : Form.AmenitiesDescription.Trim()) : null;
        entity.NearbyLandmarks = Form.IsOffPlan ? (string.IsNullOrWhiteSpace(Form.NearbyLandmarks) ? null : Form.NearbyLandmarks.Trim()) : null;

        entity.DeveloperId = Form.DeveloperId == 0 ? null : Form.DeveloperId;
        entity.IsVerified = Form.IsVerified;
        entity.SeoTitle = string.IsNullOrWhiteSpace(Form.SeoTitle) ? null : Form.SeoTitle.Trim();
        entity.SeoDescription = string.IsNullOrWhiteSpace(Form.SeoDescription) ? null : Form.SeoDescription.Trim();
        entity.ApprovalStatus = Form.ApprovalStatus;
        // Auto-publish when admin explicitly marks Approved
        if (Form.ApprovalStatus == ListingApprovalStatus.Approved) entity.IsPublished = Form.IsPublished;
        else if (Form.ApprovalStatus == ListingApprovalStatus.Rejected) entity.IsPublished = false;
        else entity.IsPublished = Form.IsPublished;
        entity.AvailabilityStatus = Form.AvailabilityStatus;
        entity.OccupancyStatus = Form.OccupancyStatus;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        // Track whether this publish is new (to trigger saved search alerts)
        var wasUnpublished = !entity.IsPublished;
        var nowPublishing = Form.IsPublished;

        await _db.SaveChangesAsync(cancellationToken);

        // Evict the home-page stats cache so the property counter updates immediately.
        _homeStats.InvalidateCache();

        // Saved search alerts — when listing goes live for the first time
        if (wasUnpublished && nowPublishing && !string.IsNullOrWhiteSpace(entity.Slug))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            _ = Task.Run(() => _alerts.NotifyMatchingUsersAsync(entity, baseUrl, CancellationToken.None));
        }

        // Send price drop alerts to users who saved this listing
        if (priceDropped && await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, cancellationToken))
        {
            var savedUsers = await _db.SavedProperties
                .Include(s => s.User)
                .Where(s => s.ListingId == entity.Id && s.User.Email != null)
                .Select(s => s.User.Email!)
                .ToListAsync(cancellationToken);

            var drop = oldPrice!.Value - Form.Price!.Value;
            var pct = Math.Round(drop / oldPrice.Value * 100, 1);
            var url = $"{Request.Scheme}://{Request.Host}/Properties/{slug}";

            foreach (var userEmail in savedUsers)
            {
                await _email.SendAsync(userEmail,
                    $"Price reduced on a property you saved — {entity.Title}",
                    $"""
                    <h3>💰 Price Drop Alert</h3>
                    <p>A property on your saved list just dropped in price.</p>
                    <p><strong>{entity.Title}</strong></p>
                    <p>Old price: <del>AED {oldPrice.Value:N0}</del><br/>
                    New price: <strong>AED {Form.Price.Value:N0}</strong> (-AED {drop:N0} / {pct}%)</p>
                    <p><a href="{url}">View property</a></p>
                    """, cancellationToken);
            }

            if (savedUsers.Count > 0)
                TempData["ToastOk"] = $"Listing saved. Price drop alerts sent to {savedUsers.Count} saved user(s).";
        }

        if (TempData["ToastOk"] == null) TempData["ToastOk"] = "Listing saved.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _db.PropertyListings.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity != null) { _db.PropertyListings.Remove(entity); await _db.SaveChangesAsync(cancellationToken); _homeStats.InvalidateCache(); }
        TempData["ToastOk"] = "Listing deleted.";
        return RedirectToPage("./Index");
    }

    private static PropertyListingForm MapFrom(PropertyListing e) => new()
    {
        Title = e.Title,
        Slug = e.Slug,
        Price = e.Price,
        Currency = e.Currency,
        OfferType = e.OfferType,
        Location = e.Location,
        PropertyCategory = e.PropertyCategory,
        PropertyType = e.PropertyType,
        Emirates = e.Emirates,
        Description = e.Description,
        Beds = e.Beds,
        Baths = e.Baths,
        AreaSqft = e.AreaSqft,
        MainImageUrl = e.MainImageUrl,
        GalleryImagesJson = ParseGalleryForEdit(e.GalleryImagesJson),
        FloorPlanUrl = e.FloorPlanUrl,
        VirtualTourUrl = e.VirtualTourUrl,
        Label = e.Label,
        AmenitiesJson = e.AmenitiesJson,
        IsOffPlan = e.IsOffPlan,
        HandoverDate = e.HandoverDate,
        PaymentPlan = e.PaymentPlan,
        CompletionPercent = e.CompletionPercent,
        Subtitle = e.Subtitle,
        ProjectStatus = e.ProjectStatus,
        ProjectAddress = e.ProjectAddress,
        UnitTypes = e.UnitTypes,
        BedroomOptions = e.BedroomOptions,
        BathroomOptions = e.BathroomOptions,
        StartingSizeSqft = e.StartingSizeSqft,
        TotalFloors = e.TotalFloors,
        TotalBuildings = e.TotalBuildings,
        TotalUnits = e.TotalUnits,
        DownPaymentPercent = e.DownPaymentPercent,
        DuringConstructionPercent = e.DuringConstructionPercent,
        OnHandoverPercent = e.OnHandoverPercent,
        KeyFeatures = e.KeyFeatures,
        AmenitiesDescription = e.AmenitiesDescription,
        NearbyLandmarks = e.NearbyLandmarks,
        DeveloperId = e.DeveloperId,
        IsVerified = e.IsVerified,
        ApprovalStatus = e.ApprovalStatus,
        RejectionReason = e.RejectionReason,
        AvailabilityStatus = e.AvailabilityStatus,
        OccupancyStatus = e.OccupancyStatus,
        SeoTitle = e.SeoTitle,
        SeoDescription = e.SeoDescription,
        IsPublished = e.IsPublished
    };

    private static string? ParseGalleryUrls(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var urls = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .Where(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToArray();
        return urls.Length == 0 ? null : JsonSerializer.Serialize(urls);
    }

    private static string? ParseGalleryForEdit(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { var arr = JsonSerializer.Deserialize<string[]>(json); return arr == null ? null : string.Join('\n', arr); }
        catch { return null; }
    }
}
