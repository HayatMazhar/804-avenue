using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Developers;

[Authorize(Policy = "AdminAccess")]
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
            WebsiteUrl = F.WebsiteUrl?.Trim(), Description = F.Description?.Trim(),
            Mission = F.Mission?.Trim(), Vision = F.Vision?.Trim(), Philosophy = F.Philosophy?.Trim(),
            YearsInBusiness = F.YearsInBusiness, TotalProjects = F.TotalProjects,
            UnitsDelivered = F.UnitsDelivered, AwardsRecognition = F.AwardsRecognition?.Trim(),
            IsActive = F.IsActive
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

        // Company profile
        [StringLength(4000)] public string? Description { get; set; }
        [StringLength(4000)] public string? Mission { get; set; }
        [StringLength(4000)] public string? Vision { get; set; }
        [StringLength(4000)] public string? Philosophy { get; set; }

        // Achievements & statistics
        [Range(0, 1000)] public int? YearsInBusiness { get; set; }
        [Range(0, 1000000)] public int? TotalProjects { get; set; }
        [Range(0, 100000000)] public int? UnitsDelivered { get; set; }
        [StringLength(4000)] public string? AwardsRecognition { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
