using Avenue804.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Api;

public class DeleteSearchModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DeleteSearchModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public IActionResult OnGet() => RedirectToPage("/Account/Dashboard");

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct = default)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var row = await _db.SavedSearches.FirstOrDefaultAsync(s => s.Id == id && s.UserId == user.Id, ct);
        if (row != null)
        {
            _db.SavedSearches.Remove(row);
            await _db.SaveChangesAsync(ct);
        }

        TempData["ToastOk"] = "Saved search removed.";
        return RedirectToPage("/Account/Dashboard");
    }
}
