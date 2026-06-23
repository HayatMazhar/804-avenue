using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Blog;

public class BlogIndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public BlogIndexModel(ApplicationDbContext db) => _db = db;

    public List<BlogPost> Posts { get; private set; } = [];

    [BindProperty(SupportsGet = true, Name = "category")]
    public BlogCategory? Category { get; set; }

    public Dictionary<BlogCategory, int> CategoryCounts { get; private set; } = [];
    public int TotalCount { get; private set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["NavActive"] = "blog";

        var allPublished = _db.BlogPosts.AsNoTracking().Where(p => p.IsPublished && p.Slug != null);

        TotalCount = await allPublished.CountAsync(ct);
        CategoryCounts = await allPublished
            .GroupBy(p => p.Category)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);

        var query = allPublished;
        if (Category.HasValue)
            query = query.Where(p => p.Category == Category.Value);

        Posts = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }
}
