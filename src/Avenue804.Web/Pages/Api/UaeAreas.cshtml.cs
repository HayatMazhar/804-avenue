using Avenue804.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Api;

public class UaeAreasModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public UaeAreasModel(ApplicationDbContext db) => _db = db;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnGetSearchAsync(string? q, string? emirate, int limit = 10, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return new JsonResult(Array.Empty<object>());

        limit = Math.Min(limit, 20);
        var query = _db.UaeAreas.AsNoTracking().Where(a => a.IsActive);

        if (!string.IsNullOrWhiteSpace(emirate))
            query = query.Where(a => a.Emirate == emirate);

        var results = await query
            .Where(a => a.Name.Contains(q) || (a.City != null && a.City.Contains(q)) || a.Emirate.Contains(q))
            .OrderByDescending(a => a.SortOrder)
            .Take(limit)
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Emirate,
                a.City,
                a.Type,
                Label = a.City != null && a.City != a.Emirate
                    ? $"{a.Name}, {a.City}, {a.Emirate}"
                    : $"{a.Name}, {a.Emirate}"
            })
            .ToListAsync(ct);

        return new JsonResult(results);
    }
}
