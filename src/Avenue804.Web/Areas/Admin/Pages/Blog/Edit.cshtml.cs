using Avenue804.Web.Data;
using Avenue804.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Avenue804.Web.Areas.Admin.Pages.Blog.CreateModel;

namespace Avenue804.Web.Areas.Admin.Pages.Blog;

[Authorize(Policy = "AdminAccess")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public BlogForm F { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "blog";
        var p = await _db.BlogPosts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (p == null) return NotFound();

        F = new()
        {
            Title = p.Title,
            Slug = p.Slug,
            Category = p.Category,
            Excerpt = p.Excerpt,
            Content = p.Content,
            Tags = p.Tags,
            CoverImageUrl = p.CoverImageUrl,
            AuthorName = p.AuthorName,
            SeoTitle = p.SeoTitle,
            SeoMetaDescription = p.SeoMetaDescription,
            FocusKeyword = p.FocusKeyword,
            IsPublished = p.IsPublished,
            PublishedAt = p.PublishedAt
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "blog";
        if (!ModelState.IsValid) return Page();

        var post = await _db.BlogPosts.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (post == null) return NotFound();

        var baseSlug = string.IsNullOrWhiteSpace(F.Slug)
            ? SlugGenerator.FromTitle(F.Title)
            : SlugGenerator.FromTitle(F.Slug.Trim(), F.Title);
        var slug = await UniqueBlogSlugAsync(_db, baseSlug, Id, ct);

        var wasPublished = post.IsPublished;

        post.Title = F.Title.Trim();
        post.Slug = slug;
        post.Category = F.Category;
        post.Excerpt = string.IsNullOrWhiteSpace(F.Excerpt) ? null : F.Excerpt.Trim();
        post.Content = F.Content?.Trim() ?? string.Empty;
        post.Tags = string.IsNullOrWhiteSpace(F.Tags) ? null : F.Tags.Trim();
        post.CoverImageUrl = string.IsNullOrWhiteSpace(F.CoverImageUrl) ? null : F.CoverImageUrl.Trim();
        post.AuthorName = string.IsNullOrWhiteSpace(F.AuthorName) ? null : F.AuthorName.Trim();
        post.SeoTitle = string.IsNullOrWhiteSpace(F.SeoTitle) ? null : F.SeoTitle.Trim();
        post.SeoMetaDescription = string.IsNullOrWhiteSpace(F.SeoMetaDescription) ? null : F.SeoMetaDescription.Trim();
        post.FocusKeyword = string.IsNullOrWhiteSpace(F.FocusKeyword) ? null : F.FocusKeyword.Trim();
        post.IsPublished = F.IsPublished;
        post.UpdatedAt = DateTimeOffset.UtcNow;

        // Set PublishedAt the first time it goes live (or accept the admin's override).
        if (F.PublishedAt.HasValue) post.PublishedAt = F.PublishedAt;
        else if (F.IsPublished && !wasPublished && !post.PublishedAt.HasValue) post.PublishedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Blog post saved.";
        return RedirectToPage(new { id = Id });
    }
}
