using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.ContentBlocks;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IReadOnlyList<ContentBlock> Rows { get; private set; } = [];

    [BindProperty(SupportsGet = true, Name = "q")]
    public string? Query { get; set; }

    /// <summary>Filter to a single page group, e.g. "home.*" or "page.property_detail.*".</summary>
    [BindProperty(SupportsGet = true, Name = "group")]
    public string? Group { get; set; }

    public List<string> AvailableGroups { get; private set; } = [];
    public int TotalCount { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "content";

        // Build the group list (first two slug segments, e.g. "home.hero") once
        AvailableGroups = await _db.ContentBlocks
            .AsNoTracking()
            .Select(b => b.Slug)
            .ToListAsync(cancellationToken)
            .ContinueWith(t => t.Result
                .Select(s => s.Split('.') is { Length: >= 2 } parts ? $"{parts[0]}.{parts[1]}" : s.Split('.')[0])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
                .ToList(), cancellationToken);

        TotalCount = await _db.ContentBlocks.CountAsync(cancellationToken);

        IQueryable<ContentBlock> q = _db.ContentBlocks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Group))
        {
            var prefix = Group.Trim();
            q = q.Where(b => b.Slug.StartsWith(prefix));
        }

        if (!string.IsNullOrWhiteSpace(Query))
        {
            var s = Query.Trim();
            q = q.Where(b => EF.Functions.Like(b.Slug, $"%{s}%")
                          || (b.Title != null && EF.Functions.Like(b.Title, $"%{s}%")));
        }

        Rows = await q.OrderBy(x => x.Slug).ToListAsync(cancellationToken);
    }
}
