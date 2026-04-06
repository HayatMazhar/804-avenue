using Avenue804.Web.Data;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Lookups;

[Authorize(Policy = "AdminOnly")]
public class ValueEditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ValueEditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public int CategoryId { get; private set; }

    public string? CategoryName { get; private set; }

    [BindProperty]
    public LookupValueForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        var row = await _db.LookupValues.AsNoTracking().Include(v => v.Category).FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (row?.Category == null)
            return NotFound();

        CategoryId = row.CategoryId;
        CategoryName = row.Category.Name;
        Form = new LookupValueForm
        {
            Code = row.Code,
            DisplayName = row.DisplayName,
            SortOrder = row.SortOrder,
            IsActive = row.IsActive,
            Metadata = row.Metadata
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        var entity = await _db.LookupValues.Include(v => v.Category).FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity?.Category == null)
            return NotFound();

        CategoryId = entity.CategoryId;
        CategoryName = entity.Category.Name;

        Form.Code = Form.Code.Trim();
        Form.DisplayName = Form.DisplayName.Trim();

        if (await _db.LookupValues.AnyAsync(
                v => v.CategoryId == entity.CategoryId && v.Id != Id && v.Code.ToLower() == Form.Code.ToLower(),
                cancellationToken))
            ModelState.AddModelError(nameof(Form.Code), "Code must be unique within this category.");

        if (!ModelState.IsValid)
            return Page();

        entity.Code = Form.Code;
        entity.DisplayName = Form.DisplayName;
        entity.SortOrder = Form.SortOrder;
        entity.IsActive = Form.IsActive;
        entity.Metadata = string.IsNullOrWhiteSpace(Form.Metadata) ? null : Form.Metadata.Trim();
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Value saved.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _db.LookupValues.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity != null)
        {
            var catId = entity.CategoryId;
            _db.LookupValues.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            TempData["ToastOk"] = "Value deleted.";
            return RedirectToPage("Values", new { categoryId = catId });
        }

        return RedirectToPage("Index");
    }
}
