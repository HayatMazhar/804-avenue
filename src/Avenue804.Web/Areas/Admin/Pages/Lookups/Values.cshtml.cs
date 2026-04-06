using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Lookups;

[Authorize(Policy = "AdminOnly")]
public class ValuesModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ValuesModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int CategoryId { get; set; }

    public LookupCategory? Category { get; private set; }

    public IReadOnlyList<LookupValue> Rows { get; private set; } = [];

    [BindProperty]
    public LookupValueForm NewValue { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        Category = await _db.LookupCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == CategoryId, cancellationToken);
        if (Category == null)
            return NotFound();

        Rows = await _db.LookupValues.AsNoTracking()
            .Where(v => v.CategoryId == CategoryId)
            .OrderBy(v => v.SortOrder)
            .ThenBy(v => v.DisplayName)
            .ToListAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        Category = await _db.LookupCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == CategoryId, cancellationToken);
        if (Category == null)
            return NotFound();

        NewValue.Code = NewValue.Code.Trim();
        NewValue.DisplayName = NewValue.DisplayName.Trim();

        if (await _db.LookupValues.AnyAsync(
                v => v.CategoryId == CategoryId && v.Code.ToLower() == NewValue.Code.ToLower(),
                cancellationToken))
            ModelState.AddModelError(nameof(NewValue.Code), "Code must be unique within this category.");

        Rows = await _db.LookupValues.AsNoTracking()
            .Where(v => v.CategoryId == CategoryId)
            .OrderBy(v => v.SortOrder)
            .ThenBy(v => v.DisplayName)
            .ToListAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        _db.LookupValues.Add(new LookupValue
        {
            CategoryId = CategoryId,
            Code = NewValue.Code,
            DisplayName = NewValue.DisplayName,
            SortOrder = NewValue.SortOrder,
            IsActive = NewValue.IsActive,
            Metadata = string.IsNullOrWhiteSpace(NewValue.Metadata) ? null : NewValue.Metadata.Trim()
        });
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Value added.";
        return RedirectToPage(new { categoryId = CategoryId });
    }
}
