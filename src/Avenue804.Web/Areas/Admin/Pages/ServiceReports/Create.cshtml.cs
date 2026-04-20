using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.ServiceReports;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;
    [BindProperty] public ReportForm F { get; set; } = new();

    public void OnGet() { ViewData["AdminSection"] = "service-reports"; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        _db.ServiceReports.Add(new ServiceReport
        {
            ClientName = F.ClientName.Trim(), ClientEmail = F.ClientEmail.Trim(),
            Title = F.Title.Trim(), VisitDate = F.VisitDate,
            ReportFileUrl = F.ReportFileUrl?.Trim(), Description = F.Description?.Trim()
        });
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Service report saved.";
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
