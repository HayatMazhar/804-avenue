using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Projects;

public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailModel(ApplicationDbContext db) => _db = db;

    public PortfolioProject? Project { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken cancellationToken = default)
    {
        ViewData["NavActive"] = "projects";
        if (string.IsNullOrWhiteSpace(slug))
            return NotFound();

        var key = slug.Trim();
        Project = await _db.PortfolioProjects.AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.IsPublished && p.Slug == key,
                cancellationToken);

        if (Project == null)
            return NotFound();

        ViewData["Title"] = Project.Title;
        ViewData["MetaDescription"] = string.IsNullOrWhiteSpace(Project.Summary)
            ? $"{Project.Title} — 804 Avenue contracting portfolio"
            : Project.Summary.Length > 200
                ? Project.Summary[..200].Trim() + "…"
                : Project.Summary;

        return Page();
    }
}
