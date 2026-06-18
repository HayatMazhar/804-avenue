using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.ProjectProgress;

[Authorize(Policy = "AdminAccess")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;
    [BindProperty] public ProjForm F { get; set; } = new();

    public void OnGet() { ViewData["AdminSection"] = "project-progress"; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var token = Guid.NewGuid().ToString("N")[..12];
        _db.ProjectProgressItems.Add(new Domain.ProjectProgress
        {
            ClientName = F.ClientName.Trim(), ClientEmail = F.ClientEmail.Trim(),
            ProjectTitle = F.ProjectTitle?.Trim(), Location = F.Location?.Trim(),
            CurrentStage = F.CurrentStage, ProgressPercent = F.ProgressPercent,
            EstimatedCompletion = F.EstimatedCompletion,
            StageNotes = F.StageNotes?.Trim(), AccessToken = token
        });
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Project added.";
        return RedirectToPage("./Index");
    }

    public class ProjForm
    {
        [Required, StringLength(200)] public string ClientName { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(256)] public string ClientEmail { get; set; } = string.Empty;
        [StringLength(300)] public string? ProjectTitle { get; set; }
        [StringLength(400)] public string? Location { get; set; }
        public ProjectStage CurrentStage { get; set; }
        [Range(0, 100)] public int ProgressPercent { get; set; }
        public DateTimeOffset? EstimatedCompletion { get; set; }
        [StringLength(2000)] public string? StageNotes { get; set; }
    }
}
