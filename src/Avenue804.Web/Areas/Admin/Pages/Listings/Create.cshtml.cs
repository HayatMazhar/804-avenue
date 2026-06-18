using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

/// <summary>
/// Creates a blank draft listing immediately and redirects to Edit, so the admin
/// sees the full form (Property Type cascade, Label/Badge, Developer, Off-plan toggle,
/// Amenities, Media uploader and SEO) the moment they click "+ New listing".
/// Drafts are not visible publicly until published.
/// </summary>
[Authorize(Policy = "AdminAccess")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        var stamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var baseSlug = $"draft-listing-{stamp}";
        var slug = await UniqueSlug.ForPropertyListingAsync(_db, baseSlug, null, cancellationToken);

        var draft = new PropertyListing
        {
            Title = "Untitled listing",
            Slug = slug,
            Currency = "AED",
            OfferType = ListingOfferType.Sale,
            ApprovalStatus = ListingApprovalStatus.Draft,
            IsPublished = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.PropertyListings.Add(draft);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Draft created — fill in the details and save.";
        return RedirectToPage("./Edit", new { id = draft.Id });
    }

    // POST kept for compatibility but no longer reachable from the slim form
    public IActionResult OnPost() => RedirectToPage("./Index");
}
