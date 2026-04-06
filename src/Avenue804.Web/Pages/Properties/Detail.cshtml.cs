using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Properties;

public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailModel(ApplicationDbContext db) => _db = db;

    public PropertyListing? Listing { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken = default)
    {
        ViewData["NavActive"] = "properties";
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var key = slug.Trim();
        Listing = await _db.PropertyListings.AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.IsPublished && p.Slug == key,
                cancellationToken);

        if (Listing == null)
            return NotFound();

        ViewData["Title"] = Listing.Title;
        ViewData["MetaDescription"] = string.IsNullOrWhiteSpace(Listing.Description)
            ? $"{Listing.Title} — 804 Avenue Properties"
            : Listing.Description.Length > 160
                ? Listing.Description[..160].Trim() + "…"
                : Listing.Description;

        return Page();
    }
}
