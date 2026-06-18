using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.OwnerRequests;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public const int PageSize = 25;

    public int PageNumber { get; set; } = 1;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public IList<OwnerListingRequest> Items { get; set; } = [];

    public async Task OnGetAsync(int p = 1, CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "owner";
        PageNumber = p < 1 ? 1 : p;
        TotalCount = await _db.OwnerListingRequests.CountAsync(cancellationToken);
        Items = await _db.OwnerListingRequests.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync(cancellationToken);
    }
}
