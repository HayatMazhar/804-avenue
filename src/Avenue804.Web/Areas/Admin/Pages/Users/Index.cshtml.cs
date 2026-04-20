using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Users;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly ApplicationDbContext _db;

    public IndexModel(UserManager<ApplicationUser> users, ApplicationDbContext db)
    {
        _users = users;
        _db = db;
    }

    public List<UserRow> Users { get; set; } = [];
    public int TotalCount { get; set; }

    public class UserRow
    {
        public string Id { get; set; } = "";
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string LoginProvider { get; set; } = "Email";
    }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "users";

        var publicUsers = await _users.Users
            .Where(u => u.IsPublicUser)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);

        TotalCount = publicUsers.Count;

        foreach (var u in publicUsers)
        {
            var logins = await _users.GetLoginsAsync(u);
            var provider = logins.FirstOrDefault()?.LoginProvider ?? "Email";

            Users.Add(new UserRow
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                EmailConfirmed = u.EmailConfirmed,
                LockoutEnabled = u.LockoutEnabled,
                LockoutEnd = u.LockoutEnd,
                CreatedAt = u.CreatedAt,
                LoginProvider = provider
            });
        }
    }

    public async Task<IActionResult> OnPostAsync(string userId, string action, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (action == "suspend")
        {
            await _users.SetLockoutEnabledAsync(user, true);
            await _users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(10));
            TempData["ToastOk"] = $"User {user.Email} suspended.";
        }
        else if (action == "activate")
        {
            await _users.SetLockoutEndDateAsync(user, null);
            TempData["ToastOk"] = $"User {user.Email} reactivated.";
        }

        return RedirectToPage();
    }
}
