using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.AreaGuides;

[Authorize(Policy = "AdminAccess")]
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
            Name = F.Name.Trim(), Slug = slug, Emirate = F.Emirate, City = F.City?.Trim(),
            HeroImageUrl = F.HeroImageUrl?.Trim(),
            ShortDescription = F.ShortDescription?.Trim(),
            Overview = F.Overview?.Trim(),
            AvgPriceSaleSqft = F.AvgPriceSaleSqft, AvgRentYearly = F.AvgRentYearly,
            PopularWith = F.PopularWith?.Trim(), NearbyLandmarks = F.NearbyLandmarks?.Trim(),
            SchoolsNearby = F.SchoolsNearby?.Trim(), TransportLinks = F.TransportLinks?.Trim(),
            Amenities = F.Amenities?.Trim(), Attractions = F.Attractions?.Trim(),
            LifestyleServices = F.LifestyleServices?.Trim(), Shopping = F.Shopping?.Trim(),
            Education = F.Education?.Trim(), Dining = F.Dining?.Trim(),
            Healthcare = F.Healthcare?.Trim(), Transportation = F.Transportation?.Trim(),
            AverageRoiPercent = F.AverageRoiPercent, RentalYieldPercent = F.RentalYieldPercent,
            InvestmentInsights = F.InvestmentInsights?.Trim(),
            IsPublished = F.IsPublished
        });
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Area guide created.";
        return RedirectToPage("./Index");
    }

    public class GuideForm
    {
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(100)] public string? Emirate { get; set; }
        [StringLength(100)] public string? City { get; set; }
        [StringLength(2000), Url] public string? HeroImageUrl { get; set; }
        [StringLength(600)] public string? ShortDescription { get; set; }
        [StringLength(20000)] public string? Overview { get; set; }
        public decimal? AvgPriceSaleSqft { get; set; }
        public decimal? AvgRentYearly { get; set; }
        [StringLength(300)] public string? PopularWith { get; set; }
        [StringLength(1000)] public string? NearbyLandmarks { get; set; }
        [StringLength(1000)] public string? SchoolsNearby { get; set; }
        [StringLength(1000)] public string? TransportLinks { get; set; }

        // Lifestyle & amenities
        [StringLength(4000)] public string? Amenities { get; set; }
        [StringLength(4000)] public string? Attractions { get; set; }
        [StringLength(4000)] public string? LifestyleServices { get; set; }
        [StringLength(4000)] public string? Shopping { get; set; }
        [StringLength(4000)] public string? Education { get; set; }
        [StringLength(4000)] public string? Dining { get; set; }
        [StringLength(4000)] public string? Healthcare { get; set; }
        [StringLength(4000)] public string? Transportation { get; set; }

        // Investment information
        [Range(0, 100)] public decimal? AverageRoiPercent { get; set; }
        [Range(0, 100)] public decimal? RentalYieldPercent { get; set; }
        [StringLength(8000)] public string? InvestmentInsights { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}
