using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.AreaGuides;

public class AreaGuidesIndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public AreaGuidesIndexModel(ApplicationDbContext db) => _db = db;
    public Dictionary<string, List<AreaGuide>> Grouped { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var guides = await _db.AreaGuides.AsNoTracking()
            .Where(g => g.IsPublished)
            .OrderBy(g => g.Emirate).ThenBy(g => g.Name)
            .ToListAsync(ct);

        Grouped = guides.GroupBy(g => g.Emirate ?? "Other")
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
