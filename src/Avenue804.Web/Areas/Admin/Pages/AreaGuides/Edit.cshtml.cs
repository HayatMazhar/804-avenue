using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Avenue804.Web.Areas.Admin.Pages.AreaGuides.CreateModel;

namespace Avenue804.Web.Areas.Admin.Pages.AreaGuides;

[Authorize(Policy = "AdminAccess")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) => _db = db;
    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public GuideForm F { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "areaguides";
        var g = await _db.AreaGuides.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (g == null) return NotFound();
        F = new()
        {
            Name = g.Name, Emirate = g.Emirate, City = g.City,
            HeroImageUrl = g.HeroImageUrl,
            ShortDescription = g.ShortDescription, Overview = g.Overview,
            AvgPriceSaleSqft = g.AvgPriceSaleSqft, AvgRentYearly = g.AvgRentYearly,
            PopularWith = g.PopularWith, NearbyLandmarks = g.NearbyLandmarks,
            SchoolsNearby = g.SchoolsNearby, TransportLinks = g.TransportLinks,
            Amenities = g.Amenities, Attractions = g.Attractions,
            LifestyleServices = g.LifestyleServices, Shopping = g.Shopping,
            Education = g.Education, Dining = g.Dining,
            Healthcare = g.Healthcare, Transportation = g.Transportation,
            AverageRoiPercent = g.AverageRoiPercent, RentalYieldPercent = g.RentalYieldPercent,
            InvestmentInsights = g.InvestmentInsights,
            IsPublished = g.IsPublished
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var g = await _db.AreaGuides.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (g == null) return NotFound();
        g.Name = F.Name.Trim(); g.Emirate = F.Emirate; g.City = F.City?.Trim();
        g.HeroImageUrl = F.HeroImageUrl?.Trim();
        g.ShortDescription = F.ShortDescription?.Trim(); g.Overview = F.Overview?.Trim();
        g.AvgPriceSaleSqft = F.AvgPriceSaleSqft; g.AvgRentYearly = F.AvgRentYearly;
        g.PopularWith = F.PopularWith?.Trim(); g.NearbyLandmarks = F.NearbyLandmarks?.Trim();
        g.SchoolsNearby = F.SchoolsNearby?.Trim(); g.TransportLinks = F.TransportLinks?.Trim();
        g.Amenities = F.Amenities?.Trim(); g.Attractions = F.Attractions?.Trim();
        g.LifestyleServices = F.LifestyleServices?.Trim(); g.Shopping = F.Shopping?.Trim();
        g.Education = F.Education?.Trim(); g.Dining = F.Dining?.Trim();
        g.Healthcare = F.Healthcare?.Trim(); g.Transportation = F.Transportation?.Trim();
        g.AverageRoiPercent = F.AverageRoiPercent; g.RentalYieldPercent = F.RentalYieldPercent;
        g.InvestmentInsights = F.InvestmentInsights?.Trim();
        g.IsPublished = F.IsPublished;
        g.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Area guide updated.";
        return RedirectToPage(new { id = Id });
    }
}
