using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminAccess")]
public class ReviewModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ReviewModel(ApplicationDbContext db) => _db = db;
    public List<PropertyListing> Items { get; set; } = [];
    public int PendingCount { get; set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "listings";
        Items = await _db.PropertyListings
            .Where(p => p.ApprovalStatus == ListingApprovalStatus.PendingReview)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);
        PendingCount = Items.Count;
    }

    public async Task<IActionResult> OnPostAsync(int id, string action, string? reason, CancellationToken ct = default)
    {
        var listing = await _db.PropertyListings.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (listing == null) return NotFound();

        if (action == "approve")
        {
            listing.ApprovalStatus = ListingApprovalStatus.Approved;
            listing.IsPublished = true;
            listing.UpdatedAt = DateTimeOffset.UtcNow;
            TempData["ToastOk"] = $"'{listing.Title}' approved and published.";
        }
        else if (action == "reject")
        {
            listing.ApprovalStatus = ListingApprovalStatus.Rejected;
            listing.IsPublished = false;
            listing.RejectionReason = reason?.Trim();
            listing.UpdatedAt = DateTimeOffset.UtcNow;
            TempData["ToastOk"] = $"'{listing.Title}' rejected.";
        }

        await _db.SaveChangesAsync(ct);
        return RedirectToPage();
    }
}
