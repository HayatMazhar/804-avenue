using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Avenue804.Web.Areas.Admin.Pages.ProjectProgress.CreateModel;

namespace Avenue804.Web.Areas.Admin.Pages.ProjectProgress;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public ProjForm F { get; set; } = new();
    public string? AccessToken { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "project-progress";
        var p = await _db.ProjectProgressItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (p == null) return NotFound();
        AccessToken = p.AccessToken;
        F = new() { ClientName = p.ClientName, ClientEmail = p.ClientEmail, ProjectTitle = p.ProjectTitle, Location = p.Location, CurrentStage = p.CurrentStage, ProgressPercent = p.ProgressPercent, EstimatedCompletion = p.EstimatedCompletion, StageNotes = p.StageNotes };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        var p = await _db.ProjectProgressItems.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (p == null) return NotFound();
        p.ClientName = F.ClientName.Trim(); p.ClientEmail = F.ClientEmail.Trim();
        p.ProjectTitle = F.ProjectTitle?.Trim(); p.Location = F.Location?.Trim();
        p.CurrentStage = F.CurrentStage; p.ProgressPercent = F.ProgressPercent;
        p.EstimatedCompletion = F.EstimatedCompletion;
        p.StageNotes = F.StageNotes?.Trim(); p.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Project updated.";
        return RedirectToPage(new { id = Id });
    }
}
