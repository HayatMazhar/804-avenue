using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Agents;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Agent> Agents { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "agents";
        Agents = await _db.Agents
            .Include(a => a.Listings)
            .OrderBy(a => a.Name)
            .ToListAsync(ct);
    }
}
