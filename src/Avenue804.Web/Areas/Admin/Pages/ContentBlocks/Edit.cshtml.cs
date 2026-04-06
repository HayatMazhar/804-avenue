using Avenue804.Web.Data;
using Avenue804.Web.Models.Admin;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Areas.Admin.Pages.ContentBlocks;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public EditModel(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string? OriginalSlug { get; private set; }

    [BindProperty]
    public ContentBlockForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "content";
        var row = await _db.ContentBlocks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (row == null)
            return NotFound();

        OriginalSlug = row.Slug;
        Form = new ContentBlockForm
        {
            Slug = row.Slug,
            Title = row.Title,
            Body = row.Body,
            IsPublished = row.IsPublished
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "content";
        var entity = await _db.ContentBlocks.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();

        OriginalSlug = entity.Slug;
        Form.Slug = Form.Slug.Trim().ToLowerInvariant().Replace(' ', '-');

        if (await _db.ContentBlocks.AnyAsync(b => b.Slug == Form.Slug && b.Id != Id, cancellationToken))
            ModelState.AddModelError(nameof(Form.Slug), "Slug must be unique.");

        if (!ModelState.IsValid)
            return Page();

        ContentBlockService.Invalidate(_cache, entity.Slug);
        entity.Slug = Form.Slug;
        entity.Title = string.IsNullOrWhiteSpace(Form.Title) ? null : Form.Title.Trim();
        entity.Body = Form.Body.Trim();
        entity.IsPublished = Form.IsPublished;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        ContentBlockService.Invalidate(_cache, entity.Slug);
        TempData["ToastOk"] = "Content block saved.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _db.ContentBlocks.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity != null)
        {
            ContentBlockService.Invalidate(_cache, entity.Slug);
            _db.ContentBlocks.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        TempData["ToastOk"] = "Content block deleted.";
        return RedirectToPage("Index");
    }
}
