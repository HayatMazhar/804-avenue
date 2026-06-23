using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Blog;

public class BlogDetailModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public BlogDetailModel(ApplicationDbContext db) => _db = db;

    public BlogPost? Post { get; private set; }
    public List<BlogPost> Related { get; private set; } = [];
    public int ReadingMinutes { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct = default)
    {
        ViewData["NavActive"] = "blog";
        if (string.IsNullOrWhiteSpace(slug)) return NotFound();

        var key = slug.Trim();
        Post = await _db.BlogPosts.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsPublished && p.Slug == key, ct);
        if (Post == null) return NotFound();

        // Rough reading-time estimate (200 wpm on a stripped-down body).
        var plain = System.Text.RegularExpressions.Regex.Replace(Post.Content ?? string.Empty, "<.*?>", " ");
        var wordCount = plain.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        ReadingMinutes = Math.Max(1, (int)Math.Ceiling(wordCount / 200d));

        Related = await _db.BlogPosts.AsNoTracking()
            .Where(p => p.IsPublished && p.Id != Post.Id && p.Category == Post.Category)
            .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
            .Take(3)
            .ToListAsync(ct);

        ViewData["Title"] = string.IsNullOrWhiteSpace(Post.SeoTitle) ? Post.Title : Post.SeoTitle;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(Post.SeoMetaDescription)
            ? Post.SeoMetaDescription
            : Post.Excerpt;
        ViewData["OgImage"] = Post.CoverImageUrl;

        return Page();
    }
}
