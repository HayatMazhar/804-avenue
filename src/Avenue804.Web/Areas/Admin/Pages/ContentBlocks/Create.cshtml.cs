using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Admin;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Areas.Admin.Pages.ContentBlocks;

[Authorize(Policy = "AdminAccess")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public CreateModel(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [BindProperty]
    public ContentBlockForm Form { get; set; } = new();

    public void OnGet()
    {
        ViewData["AdminSection"] = "content";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "content";
        Form.Slug = Form.Slug.Trim().ToLowerInvariant().Replace(' ', '-');
        if (await _db.ContentBlocks.AnyAsync(b => b.Slug == Form.Slug, cancellationToken))
            ModelState.AddModelError(nameof(Form.Slug), "Slug must be unique.");

        if (!ModelState.IsValid)
            return Page();

        _db.ContentBlocks.Add(new ContentBlock
        {
            Slug = Form.Slug,
            Title = string.IsNullOrWhiteSpace(Form.Title) ? null : Form.Title.Trim(),
            Body = Form.Body.Trim(),
            IsPublished = Form.IsPublished,
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        ContentBlockService.Invalidate(_cache, Form.Slug);
        TempData["ToastOk"] = "Content block created.";
        return RedirectToPage("Index");
    }
}
