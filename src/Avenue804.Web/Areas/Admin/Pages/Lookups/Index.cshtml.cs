using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Lookups;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public IReadOnlyList<CategoryRow> Categories { get; private set; } = [];

    public sealed class CategoryRow
    {
        public int Id { get; init; }
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public int ValueCount { get; init; }
    }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        var cats = await _db.LookupCategories.AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
        var counts = await _db.LookupValues.AsNoTracking()
            .GroupBy(v => v.CategoryId)
            .Select(g => new { Id = g.Key, N = g.Count() })
            .ToDictionaryAsync(x => x.Id, x => x.N, cancellationToken);
        Categories = cats.Select(c => new CategoryRow
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            ValueCount = counts.GetValueOrDefault(c.Id)
        }).ToList();
    }
}
