using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.AreaGuides;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public List<AreaGuide> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "areaguides";
        Items = await _db.AreaGuides.OrderBy(g => g.Emirate).ThenBy(g => g.Name).ToListAsync(ct);
    }
}
