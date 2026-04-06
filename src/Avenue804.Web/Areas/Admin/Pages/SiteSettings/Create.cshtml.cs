using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Admin;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.SiteSettings;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISiteBrandingService _branding;

    public CreateModel(ApplicationDbContext db, ISiteBrandingService branding)
    {
        _db = db;
        _branding = branding;
    }

    [BindProperty]
    public SiteSettingForm Form { get; set; } = new();

    public IReadOnlyList<SelectListItem> KeyChoices { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "settings";
        await LoadKeyChoicesAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "settings";
        await LoadKeyChoicesAsync(cancellationToken);
        Form.Key = Form.Key.Trim();
        if (string.IsNullOrWhiteSpace(Form.Key) || Form.Key.Length > 120)
            ModelState.AddModelError(nameof(Form.Key), "Key is required (max 120 characters).");

        if (await _db.SiteSettings.AnyAsync(s => s.Key.ToLower() == Form.Key.ToLower(), cancellationToken))
            ModelState.AddModelError(nameof(Form.Key), "This key already exists in the database.");

        if (!ModelState.IsValid)
            return Page();

        _db.SiteSettings.Add(new SiteSetting
        {
            Key = Form.Key.Trim(),
            Value = Form.Value.Trim(),
            Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync(cancellationToken);
        _branding.InvalidateCache();
        TempData["ToastOk"] = "Setting created.";
        return RedirectToPage("Index");
    }

    private async Task LoadKeyChoicesAsync(CancellationToken cancellationToken)
    {
        var used = await _db.SiteSettings.Select(s => s.Key.ToLower()).ToListAsync(cancellationToken);
        var usedSet = used.ToHashSet();
        KeyChoices = SiteSettingKeys.AllDefinedKeys
            .Where(k => !usedSet.Contains(k.ToLowerInvariant()))
            .Select(k => new SelectListItem(k, k))
            .ToList();
    }
}
