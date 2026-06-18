using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.PropertyInquiries;

[Authorize(Policy = "AdminAccess")]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailModel(ApplicationDbContext db) => _db = db;

    public PropertyListingInquiry? Row { get; private set; }

    [BindProperty]
    public InquiryStatus Status { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "property-inquiries";
        Row = await _db.PropertyListingInquiries.AsNoTracking()
            .Include(x => x.IAmLookup)
            .Include(x => x.WantToLookup)
            .Include(x => x.PropertyTypeLookup)
            .Include(x => x.PropertyDetailLookup)
            .Include(x => x.LocationLookup)
            .Include(x => x.BudgetLookup)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (Row == null)
            return NotFound();
        Status = Row.Status;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.PropertyListingInquiries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return NotFound();

        entity.Status = Status;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Property inquiry updated.";
        return RedirectToPage(new { id });
    }
}
