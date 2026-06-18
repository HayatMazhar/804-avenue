using System.Text;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public IList<PropertyListing> Items { get; set; } = [];
    public int PendingReviewCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string? export, CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        PendingReviewCount = await _db.PropertyListings.CountAsync(p => p.ApprovalStatus == ListingApprovalStatus.PendingReview, cancellationToken);

        Items = await _db.PropertyListings.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (export == "csv")
        {
            var csv = new StringBuilder();
            csv.AppendLine("Id,Title,Slug,Price,Currency,OfferType,Location,Beds,Baths,AreaSqft,IsOffPlan,HandoverDate,ViewCount,IsPublished,CreatedAt");
            foreach (var p in Items)
                csv.AppendLine($"{p.Id},\"{p.Title}\",{p.Slug},{p.Price},{p.Currency},{p.OfferType},\"{p.Location}\",{p.Beds},{p.Baths},{p.AreaSqft},{p.IsOffPlan},{p.HandoverDate},{p.ViewCount},{p.IsPublished},{p.CreatedAt:yyyy-MM-dd}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"listings-{DateTimeOffset.UtcNow:yyyy-MM-dd}.csv");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBulkPublishAsync(List<int> ids, bool publish, CancellationToken ct = default)
    {
        if (ids.Count == 0) return RedirectToPage();
        await _db.PropertyListings
            .Where(p => ids.Contains(p.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsPublished, publish)
                .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow), ct);
        TempData["ToastOk"] = $"{ids.Count} listing(s) {(publish ? "published" : "unpublished")}.";
        return RedirectToPage();
    }
}
