using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Lookups;

[Authorize(Policy = "AdminOnly")]
public class CategoryEditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CategoryEditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int? Id { get; set; }

    [BindProperty]
    public LookupCategoryForm Form { get; set; } = new();

    public bool IsNew => Id is null or 0;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        if (IsNew)
            return Page();

        var row = await _db.LookupCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (row == null)
            return NotFound();

        Form = new LookupCategoryForm
        {
            Code = row.Code,
            Name = row.Name,
            SortOrder = row.SortOrder
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "lookups";
        Form.Code = Form.Code.Trim();
        Form.Name = Form.Name.Trim();

        if (IsNew)
        {
            if (await _db.LookupCategories.AnyAsync(c => c.Code.ToLower() == Form.Code.ToLower(), cancellationToken))
                ModelState.AddModelError(nameof(Form.Code), "Code must be unique.");
        }

        if (!ModelState.IsValid)
            return Page();

        if (IsNew)
        {
            _db.LookupCategories.Add(new LookupCategory
            {
                Code = Form.Code,
                Name = Form.Name,
                SortOrder = Form.SortOrder
            });
            await _db.SaveChangesAsync(cancellationToken);
            TempData["ToastOk"] = "Category created.";
            return RedirectToPage("Index");
        }

        var entity = await _db.LookupCategories.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();

        entity.Name = Form.Name;
        entity.SortOrder = Form.SortOrder;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Category saved.";
        return RedirectToPage(new { id = Id });
    }
}
