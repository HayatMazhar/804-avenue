using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.AreaGuides;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) => _db = db;
    [BindProperty] public GuideForm F { get; set; } = new();

    public void OnGet() { ViewData["AdminSection"] = "areaguides"; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var slug = F.Name.ToLowerInvariant().Replace(" ", "-").Replace("'", "");
        _db.AreaGuides.Add(new AreaGuide
        {
            Name = F.Name.Trim(), Slug = slug, Emirate = F.Emirate, HeroImageUrl = F.HeroImageUrl?.Trim(),
            Overview = F.Overview?.Trim(), AvgPriceSaleSqft = F.AvgPriceSaleSqft, AvgRentYearly = F.AvgRentYearly,
            PopularWith = F.PopularWith?.Trim(), NearbyLandmarks = F.NearbyLandmarks?.Trim(),
            SchoolsNearby = F.SchoolsNearby?.Trim(), TransportLinks = F.TransportLinks?.Trim(), IsPublished = F.IsPublished
        });
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Area guide created.";
        return RedirectToPage("./Index");
    }

    public class GuideForm
    {
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(100)] public string? Emirate { get; set; }
        [StringLength(2000), Url] public string? HeroImageUrl { get; set; }
        [StringLength(20000)] public string? Overview { get; set; }
        public decimal? AvgPriceSaleSqft { get; set; }
        public decimal? AvgRentYearly { get; set; }
        [StringLength(300)] public string? PopularWith { get; set; }
        [StringLength(1000)] public string? NearbyLandmarks { get; set; }
        [StringLength(1000)] public string? SchoolsNearby { get; set; }
        [StringLength(1000)] public string? TransportLinks { get; set; }
        public bool IsPublished { get; set; } = true;
    }
}
