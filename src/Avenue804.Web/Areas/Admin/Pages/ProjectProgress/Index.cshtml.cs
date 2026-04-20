using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.ProjectProgress;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public List<Domain.ProjectProgress> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "project-progress";
        Items = await _db.ProjectProgressItems.OrderByDescending(p => p.UpdatedAt).ToListAsync(ct);
    }
}
