using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Avenue804.Web.Areas.Admin.Pages.Developers.CreateModel;

namespace Avenue804.Web.Areas.Admin.Pages.Developers;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public DevForm F { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "developers";
        var d = await _db.Developers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (d == null) return NotFound();
        F = new() { Name = d.Name, Headquarters = d.Headquarters, EstablishedYear = d.EstablishedYear, LogoUrl = d.LogoUrl, WebsiteUrl = d.WebsiteUrl, Description = d.Description, IsActive = d.IsActive };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var d = await _db.Developers.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (d == null) return NotFound();
        d.Name = F.Name.Trim(); d.Headquarters = F.Headquarters?.Trim();
        d.EstablishedYear = F.EstablishedYear; d.LogoUrl = F.LogoUrl?.Trim();
        d.WebsiteUrl = F.WebsiteUrl?.Trim(); d.Description = F.Description?.Trim(); d.IsActive = F.IsActive;
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Developer updated.";
        return RedirectToPage(new { id = Id });
    }
}
