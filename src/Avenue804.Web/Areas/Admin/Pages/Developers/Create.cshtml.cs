using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Developers;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public DevForm F { get; set; } = new();

    public void OnGet() { ViewData["AdminSection"] = "developers"; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var slug = F.Name.ToLowerInvariant().Replace(" ", "-").Replace("&", "and");
        _db.Developers.Add(new Developer
        {
            Name = F.Name.Trim(), Slug = slug, Headquarters = F.Headquarters?.Trim(),
            EstablishedYear = F.EstablishedYear, LogoUrl = F.LogoUrl?.Trim(),
            WebsiteUrl = F.WebsiteUrl?.Trim(), Description = F.Description?.Trim(), IsActive = F.IsActive
        });
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Developer added.";
        return RedirectToPage("./Index");
    }

    public class DevForm
    {
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(200)] public string? Headquarters { get; set; }
        public int? EstablishedYear { get; set; }
        [StringLength(2000), Url] public string? LogoUrl { get; set; }
        [StringLength(2000), Url] public string? WebsiteUrl { get; set; }
        [StringLength(4000)] public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
