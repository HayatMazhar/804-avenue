using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Blog;

[Authorize(Policy = "AdminAccess")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public BlogForm F { get; set; } = new();

    public void OnGet()
    {
        ViewData["AdminSection"] = "blog";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "blog";
        if (!ModelState.IsValid) return Page();

        var baseSlug = string.IsNullOrWhiteSpace(F.Slug)
            ? SlugGenerator.FromTitle(F.Title)
            : SlugGenerator.FromTitle(F.Slug.Trim(), F.Title);
        var slug = await UniqueBlogSlugAsync(_db, baseSlug, null, ct);

        var now = DateTimeOffset.UtcNow;
        var post = new BlogPost
        {
            Title = F.Title.Trim(),
            Slug = slug,
            Category = F.Category,
            Excerpt = string.IsNullOrWhiteSpace(F.Excerpt) ? null : F.Excerpt.Trim(),
            Content = F.Content?.Trim() ?? string.Empty,
            Tags = string.IsNullOrWhiteSpace(F.Tags) ? null : F.Tags.Trim(),
            CoverImageUrl = string.IsNullOrWhiteSpace(F.CoverImageUrl) ? null : F.CoverImageUrl.Trim(),
            AuthorName = string.IsNullOrWhiteSpace(F.AuthorName) ? null : F.AuthorName.Trim(),
            SeoTitle = string.IsNullOrWhiteSpace(F.SeoTitle) ? null : F.SeoTitle.Trim(),
            SeoMetaDescription = string.IsNullOrWhiteSpace(F.SeoMetaDescription) ? null : F.SeoMetaDescription.Trim(),
            FocusKeyword = string.IsNullOrWhiteSpace(F.FocusKeyword) ? null : F.FocusKeyword.Trim(),
            IsPublished = F.IsPublished,
            CreatedAt = now,
            PublishedAt = F.IsPublished ? (F.PublishedAt ?? now) : F.PublishedAt
        };

        _db.BlogPosts.Add(post);
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Blog post created.";
        return RedirectToPage("./Edit", new { id = post.Id });
    }

    /// <summary>Allocate a unique slug for BlogPost using the same allocator pattern as PropertyListing / PortfolioProject.</summary>
    internal static async Task<string> UniqueBlogSlugAsync(ApplicationDbContext db, string baseSlug, int? excludeId, CancellationToken ct)
    {
        var root = string.IsNullOrWhiteSpace(baseSlug) ? "post" : baseSlug;
        for (var i = 0; i < 10_000; i++)
        {
            var candidate = i == 0 ? root : $"{root}-{i + 1}";
            var q = db.BlogPosts.Where(p => p.Slug == candidate);
            if (excludeId.HasValue) q = q.Where(p => p.Id != excludeId.Value);
            if (!await q.AnyAsync(ct)) return candidate;
        }
        throw new InvalidOperationException("Could not allocate a unique blog slug.");
    }

    public class BlogForm
    {
        [Required, StringLength(250)] public string Title { get; set; } = string.Empty;
        [StringLength(250)] public string? Slug { get; set; }
        public BlogCategory Category { get; set; } = BlogCategory.RealEstateNews;
        [StringLength(1000)] public string? Excerpt { get; set; }
        [Required, StringLength(100000)] public string Content { get; set; } = string.Empty;
        [StringLength(1000)] public string? Tags { get; set; }
        [StringLength(2000), Url] public string? CoverImageUrl { get; set; }
        [StringLength(200)] public string? AuthorName { get; set; }

        [StringLength(60)] public string? SeoTitle { get; set; }
        [StringLength(200)] public string? SeoMetaDescription { get; set; }
        [StringLength(100)] public string? FocusKeyword { get; set; }

        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
    }
}
