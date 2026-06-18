using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Inquiries;

[Authorize(Policy = "AdminAccess")]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailModel(ApplicationDbContext db) => _db = db;

    public Inquiry? Row { get; private set; }

    [BindProperty]
    public InquiryStatus Status { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "inquiries";
        Row = await _db.Inquiries.AsNoTracking()
            .Include(x => x.TopicLookup)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (Row == null)
            return NotFound();
        Status = Row.Status;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Inquiries.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
            return NotFound();

        entity.Status = Status;
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Inquiry updated.";
        return RedirectToPage(new { id });
    }
}
