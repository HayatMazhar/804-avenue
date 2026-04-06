using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.ContentBlocks;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IReadOnlyList<ContentBlock> Rows { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "content";
        Rows = await _db.ContentBlocks.AsNoTracking()
            .OrderBy(x => x.Slug)
            .ToListAsync(cancellationToken);
    }
}
