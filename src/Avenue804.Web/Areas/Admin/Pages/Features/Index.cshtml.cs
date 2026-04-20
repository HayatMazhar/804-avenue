using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Features;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IFeatureFlagService _flags;

    public IndexModel(ApplicationDbContext db, IFeatureFlagService flags)
    {
        _db = db;
        _flags = flags;
    }

    public IReadOnlyDictionary<string, bool> Flags { get; private set; } = new Dictionary<string, bool>();

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "features";
        Flags = await _flags.GetAllAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(string flag, bool enable, CancellationToken ct = default)
    {
        // Validate flag name
        if (!FeatureFlags.All.Any(f => f.Flag == flag))
            return BadRequest();

        var dbKey = FeatureFlags.DbKey(flag);
        var existing = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == dbKey, ct);

        if (existing == null)
        {
            _db.SiteSettings.Add(new SiteSetting
            {
                Key = dbKey,
                Value = enable ? "true" : "false",
                Description = FeatureFlags.All.FirstOrDefault(f => f.Flag == flag).Description,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existing.Value = enable ? "true" : "false";
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        _flags.InvalidateCache();

        TempData["ToastOk"] = $"{FeatureFlags.All.FirstOrDefault(f => f.Flag == flag).Label} {(enable ? "enabled" : "disabled")}.";
        return RedirectToPage();
    }
}
