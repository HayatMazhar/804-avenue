using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IList<PropertyListing> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        Items = await _db.PropertyListings.AsNoTracking()
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
