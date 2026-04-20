using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages;

public class ProjectTrackerModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ProjectTrackerModel(ApplicationDbContext db) => _db = db;
    public Domain.ProjectProgress? Project { get; private set; }

    public async Task<IActionResult> OnGetAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return NotFound();
        Project = await _db.ProjectProgressItems.AsNoTracking()
            .FirstOrDefaultAsync(p => p.AccessToken == token.Trim(), ct);
        if (Project == null) return NotFound();
        return Page();
    }
}
