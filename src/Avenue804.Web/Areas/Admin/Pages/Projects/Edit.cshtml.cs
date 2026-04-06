using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Projects;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public PortfolioProjectForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "projects";
        var entity = await _db.PortfolioProjects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();

        Form = MapFrom(entity);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "projects";
        var entity = await _db.PortfolioProjects.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();

        if (!ModelState.IsValid)
            return Page();

        var baseSlug = string.IsNullOrWhiteSpace(Form.Slug)
            ? SlugGenerator.FromTitle(Form.Title)
            : SlugGenerator.FromTitle(Form.Slug.Trim(), Form.Title);
        var slug = await UniqueSlug.ForPortfolioProjectAsync(_db, baseSlug, Id, cancellationToken);

        entity.Title = Form.Title.Trim();
        entity.Slug = slug;
        entity.Summary = string.IsNullOrWhiteSpace(Form.Summary) ? null : Form.Summary.Trim();
        entity.Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim();
        entity.CoverImageUrl = string.IsNullOrWhiteSpace(Form.CoverImageUrl) ? null : Form.CoverImageUrl.Trim();
        entity.IsPublished = Form.IsPublished;

        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Project saved.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _db.PortfolioProjects.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity != null)
        {
            _db.PortfolioProjects.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        TempData["ToastOk"] = "Project deleted.";
        return RedirectToPage("./Index");
    }

    private static PortfolioProjectForm MapFrom(PortfolioProject e) => new()
    {
        Title = e.Title,
        Slug = e.Slug,
        Summary = e.Summary,
        Description = e.Description,
        CoverImageUrl = e.CoverImageUrl,
        IsPublished = e.IsPublished
    };
}
