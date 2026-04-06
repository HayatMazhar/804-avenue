using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Projects;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IReadOnlyList<PortfolioProject> Projects { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["NavActive"] = "projects";
        Projects = await _db.PortfolioProjects.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
