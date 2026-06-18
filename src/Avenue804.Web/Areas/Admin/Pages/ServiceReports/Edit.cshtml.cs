using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.ServiceReports;

[Authorize(Policy = "AdminAccess")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public ReportForm F { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "service-reports";
        var r = await _db.ServiceReports.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (r == null) return NotFound();
        F = new()
        {
            ClientName = r.ClientName,
            ClientEmail = r.ClientEmail,
            Title = r.Title,
            VisitDate = r.VisitDate,
            ReportFileUrl = r.ReportFileUrl,
            Description = r.Description
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "service-reports";
        if (!ModelState.IsValid) return Page();
        var r = await _db.ServiceReports.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (r == null) return NotFound();
        r.ClientName = F.ClientName.Trim();
        r.ClientEmail = F.ClientEmail.Trim();
        r.Title = F.Title.Trim();
        r.VisitDate = F.VisitDate;
        r.ReportFileUrl = string.IsNullOrWhiteSpace(F.ReportFileUrl) ? null : F.ReportFileUrl.Trim();
        r.Description = string.IsNullOrWhiteSpace(F.Description) ? null : F.Description.Trim();
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Service report updated.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken ct = default)
    {
        var r = await _db.ServiceReports.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (r != null) { _db.ServiceReports.Remove(r); await _db.SaveChangesAsync(ct); }
        TempData["ToastOk"] = "Service report deleted.";
        return RedirectToPage("./Index");
    }

    public class ReportForm
    {
        [Required, StringLength(200)] public string ClientName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(256)] public string ClientEmail { get; set; } = string.Empty;
        [Required, StringLength(300)] public string Title { get; set; } = string.Empty;
        public DateTimeOffset VisitDate { get; set; } = DateTimeOffset.UtcNow;
        [StringLength(2000), Url] public string? ReportFileUrl { get; set; }
        [StringLength(4000)] public string? Description { get; set; }
    }
}
