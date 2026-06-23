using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Blog;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<BlogPost> Items { get; set; } = [];

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public BlogCategory? Category { get; set; }
    [BindProperty(SupportsGet = true)] public bool? Published { get; set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "blog";

        var query = _db.BlogPosts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            var k = Q.Trim();
            query = query.Where(p => p.Title.Contains(k) || (p.Excerpt != null && p.Excerpt.Contains(k)) || (p.Tags != null && p.Tags.Contains(k)));
        }

        if (Category.HasValue)
            query = query.Where(p => p.Category == Category.Value);

        if (Published.HasValue)
            query = query.Where(p => p.IsPublished == Published.Value);

        Items = await query
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct = default)
    {
        var post = await _db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post != null)
        {
            _db.BlogPosts.Remove(post);
            await _db.SaveChangesAsync(ct);
            TempData["ToastOk"] = "Blog post deleted.";
        }
        return RedirectToPage();
    }
}
