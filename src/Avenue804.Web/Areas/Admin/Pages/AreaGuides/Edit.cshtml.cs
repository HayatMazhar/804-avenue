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
        F = new() { Name = g.Name, Emirate = g.Emirate, HeroImageUrl = g.HeroImageUrl, Overview = g.Overview, AvgPriceSaleSqft = g.AvgPriceSaleSqft, AvgRentYearly = g.AvgRentYearly, PopularWith = g.PopularWith, NearbyLandmarks = g.NearbyLandmarks, SchoolsNearby = g.SchoolsNearby, TransportLinks = g.TransportLinks, IsPublished = g.IsPublished };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid) return Page();
        var g = await _db.AreaGuides.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (g == null) return NotFound();
        g.Name = F.Name.Trim(); g.Emirate = F.Emirate; g.HeroImageUrl = F.HeroImageUrl?.Trim();
        g.Overview = F.Overview?.Trim(); g.AvgPriceSaleSqft = F.AvgPriceSaleSqft; g.AvgRentYearly = F.AvgRentYearly;
        g.PopularWith = F.PopularWith?.Trim(); g.NearbyLandmarks = F.NearbyLandmarks?.Trim();
        g.SchoolsNearby = F.SchoolsNearby?.Trim(); g.TransportLinks = F.TransportLinks?.Trim(); g.IsPublished = F.IsPublished;
        g.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Area guide updated.";
        return RedirectToPage(new { id = Id });
    }
}
