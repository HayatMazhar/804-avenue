using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Avenue804.Web.Areas.Admin.Pages.Testimonials.CreateModel;

namespace Avenue804.Web.Areas.Admin.Pages.Testimonials;

[Authorize(Policy = "AdminAccess")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public TestimonialForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "testimonials";
        var t = await _db.Testimonials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (t == null) return NotFound();
        Form = new() { AuthorName = t.AuthorName, AuthorRole = t.AuthorRole, Quote = t.Quote, Stars = t.Stars, SortOrder = t.SortOrder, IsActive = t.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (t == null) return NotFound();
        t.AuthorName = Form.AuthorName.Trim(); t.AuthorRole = Form.AuthorRole?.Trim();
        t.Quote = Form.Quote.Trim(); t.Stars = Form.Stars; t.SortOrder = Form.SortOrder; t.IsActive = Form.IsActive;
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Testimonial updated.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken ct = default)
    {
        var t = await _db.Testimonials.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (t != null) { _db.Testimonials.Remove(t); await _db.SaveChangesAsync(ct); }
        TempData["ToastOk"] = "Testimonial deleted.";
        return RedirectToPage("./Index");
    }
}
