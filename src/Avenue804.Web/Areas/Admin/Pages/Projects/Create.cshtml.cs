using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Projects;

[Authorize(Policy = "AdminAccess")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public PortfolioProjectForm Form { get; set; } = new();

    public void OnGet()
    {
        ViewData["AdminSection"] = "projects";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "projects";
        if (!ModelState.IsValid)
            return Page();

        var baseSlug = string.IsNullOrWhiteSpace(Form.Slug)
            ? SlugGenerator.FromTitle(Form.Title)
            : SlugGenerator.FromTitle(Form.Slug.Trim(), Form.Title);
        var slug = await UniqueSlug.ForPortfolioProjectAsync(_db, baseSlug, null, cancellationToken);

        var entity = new PortfolioProject
        {
            Title = Form.Title.Trim(),
            Slug = slug,
            Summary = string.IsNullOrWhiteSpace(Form.Summary) ? null : Form.Summary.Trim(),
            Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim(),
            CoverImageUrl = string.IsNullOrWhiteSpace(Form.CoverImageUrl) ? null : Form.CoverImageUrl.Trim(),
            IsPublished = Form.IsPublished,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.PortfolioProjects.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Project created.";
        return RedirectToPage("./Edit", new { id = entity.Id });
    }
}
