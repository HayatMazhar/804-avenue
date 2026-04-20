using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Api;

public class SaveSearchModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public SaveSearchModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public IActionResult OnGet() => RedirectToPage("/Properties/Index");

    public async Task<IActionResult> OnPostAsync(
        string? offer, string? location, string? type, string? budget, string? q,
        CancellationToken ct = default)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            TempData["ToastError"] = "Sign in to save searches.";
            return RedirectToPage("/Account/Login");
        }

        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Prevent duplicates
        var exists = await _db.SavedSearches.AnyAsync(s =>
            s.UserId == user.Id &&
            s.Offer == offer &&
            s.Location == location &&
            s.PropertyType == type &&
            s.Budget == budget &&
            s.Keyword == q, ct);

        if (!exists)
        {
            _db.SavedSearches.Add(new SavedSearch
            {
                UserId = user.Id,
                Offer = offer,
                Location = location,
                PropertyType = type,
                Budget = budget,
                Keyword = q,
                Label = BuildLabel(offer, location, type, budget, q),
                AlertEnabled = true
            });
            await _db.SaveChangesAsync(ct);
            TempData["ToastOk"] = "Search saved! You'll be notified when matching properties are listed.";
        }
        else
        {
            TempData["ToastOk"] = "You already have this search saved.";
        }

        return RedirectToPage("/Properties/Index", new { offer, location, type, budget, q });
    }

    private static string BuildLabel(string? offer, string? location, string? type, string? budget, string? q)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(type)) parts.Add(type);
        if (!string.IsNullOrWhiteSpace(location)) parts.Add(location);
        if (!string.IsNullOrWhiteSpace(offer)) parts.Add(offer == "sale" ? "for sale" : "for rent");
        if (!string.IsNullOrWhiteSpace(budget)) parts.Add(budget);
        if (!string.IsNullOrWhiteSpace(q)) parts.Add($"\"{q}\"");
        return parts.Count > 0 ? string.Join(", ", parts) : "All properties";
    }
}
