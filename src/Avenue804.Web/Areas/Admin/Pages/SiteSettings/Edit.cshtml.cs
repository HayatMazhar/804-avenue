using Avenue804.Web.Data;
using Avenue804.Web.Models.Admin;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.SiteSettings;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly ISiteBrandingService _branding;

    public EditModel(ApplicationDbContext db, ISiteBrandingService branding)
    {
        _db = db;
        _branding = branding;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string? KeyName { get; private set; }

    [BindProperty]
    public SiteSettingForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "settings";
        var row = await _db.SiteSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (row == null)
            return NotFound();
        KeyName = row.Key;
        Form = new SiteSettingForm { Key = row.Key, Value = row.Value, Description = row.Description };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "settings";
        var entity = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();
        KeyName = entity.Key;

        if (!ModelState.IsValid)
            return Page();

        entity.Value = Form.Value.Trim();
        entity.Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim();
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        _branding.InvalidateCache();
        TempData["ToastOk"] = "Setting saved.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity != null)
        {
            _db.SiteSettings.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            _branding.InvalidateCache();
        }

        TempData["ToastOk"] = "Setting removed.";
        return RedirectToPage("Index");
    }
}
