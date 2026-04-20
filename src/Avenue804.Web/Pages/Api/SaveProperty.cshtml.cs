using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Api;

public class SavePropertyModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public SavePropertyModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(int listingId, bool save, string? returnUrl = null, CancellationToken ct = default)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            if (Request.Headers["Accept"].ToString().Contains("json"))
                return new JsonResult(new { error = "unauthenticated" }) { StatusCode = 401 };
            return RedirectToPage("/Account/Login", new { returnUrl = returnUrl ?? Request.Headers["Referer"].ToString() });
        }

        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var existing = await _db.SavedProperties
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.ListingId == listingId, ct);

        if (save && existing == null)
        {
            _db.SavedProperties.Add(new SavedProperty { UserId = user.Id, ListingId = listingId });
            await _db.SaveChangesAsync(ct);
        }
        else if (!save && existing != null)
        {
            _db.SavedProperties.Remove(existing);
            await _db.SaveChangesAsync(ct);
        }

        if (Request.Headers["Accept"].ToString().Contains("json"))
            return new JsonResult(new { saved = save });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToPage("/Account/Dashboard");
    }
}
