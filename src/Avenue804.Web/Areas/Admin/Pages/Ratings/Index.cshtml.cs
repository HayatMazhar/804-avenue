using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Ratings;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<PropertyRating> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "ratings";
        Items = await _db.PropertyRatings
            .Include(r => r.User)
            .Include(r => r.Listing)
            .OrderBy(r => r.IsApproved)
            .ThenByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(int id, string action, CancellationToken ct = default)
    {
        var rating = await _db.PropertyRatings.FindAsync(new object[] { id }, ct);
        if (rating == null) return NotFound();

        if (action == "approve") rating.IsApproved = true;
        else if (action == "delete") _db.PropertyRatings.Remove(rating);

        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = action == "approve" ? "Review approved." : "Review deleted.";
        return RedirectToPage();
    }
}
