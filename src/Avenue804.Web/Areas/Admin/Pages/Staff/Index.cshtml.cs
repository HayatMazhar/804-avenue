using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Staff;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;

    public IndexModel(UserManager<ApplicationUser> users) => _users = users;

    public List<StaffRow> Staff { get; set; } = [];

    public class StaffRow
    {
        public string Id { get; set; } = "";
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = [];
        public bool IsLockedOut { get; set; }
    }

    // ── Create form ─────────────────────────────────────────
    [BindProperty]
    public CreateInput Create { get; set; } = new();

    public class CreateInput
    {
        [Required, MaxLength(80)]
        public string DisplayName { get; set; } = "";

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = "";

        [Required, MinLength(8), DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        public string Role { get; set; } = SeedData.ContentEditorRole;
    }

    // ── Available roles ──────────────────────────────────────
    public static string[] AvailableRoles => SeedData.AllAdminRoles;

    // ─────────────────────────────────────────────────────────

    public async Task OnGetAsync()
    {
        ViewData["AdminSection"] = "staff";
        ViewData["Title"] = "Staff Accounts";
        await LoadStaffAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ViewData["AdminSection"] = "staff";
        ViewData["Title"] = "Staff Accounts";

        if (!ModelState.IsValid)
        {
            await LoadStaffAsync();
            return Page();
        }

        if (!SeedData.AllAdminRoles.Contains(Create.Role, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Create.Role", "Invalid role selected.");
            await LoadStaffAsync();
            return Page();
        }

        var existing = await _users.FindByEmailAsync(Create.Email.Trim());
        if (existing != null)
        {
            ModelState.AddModelError("Create.Email", "An account with this email already exists.");
            await LoadStaffAsync();
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Create.Email.Trim(),
            Email = Create.Email.Trim(),
            DisplayName = Create.DisplayName.Trim(),
            EmailConfirmed = true,
            IsPublicUser = false
        };

        var result = await _users.CreateAsync(user, Create.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
            await LoadStaffAsync();
            return Page();
        }

        await _users.AddToRoleAsync(user, Create.Role);
        TempData["ToastOk"] = $"Staff account {user.Email} created with role {Create.Role}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(string userId, string newRole)
    {
        if (!SeedData.AllAdminRoles.Contains(newRole, StringComparer.OrdinalIgnoreCase))
            return BadRequest();

        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var current = await _users.GetRolesAsync(user);
        if (current.Any())
            await _users.RemoveFromRolesAsync(user, current);
        await _users.AddToRoleAsync(user, newRole);

        TempData["ToastOk"] = $"{user.Email} role changed to {newRole}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 8)
        {
            TempData["ToastOk"] = "Password must be at least 8 characters.";
            return RedirectToPage();
        }

        var token = await _users.GeneratePasswordResetTokenAsync(user);
        var result = await _users.ResetPasswordAsync(user, token, newPassword);
        TempData["ToastOk"] = result.Succeeded
            ? $"Password reset for {user.Email}."
            : $"Failed: {string.Join("; ", result.Errors.Select(e => e.Description))}";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleLockAsync(string userId, string action)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Prevent self-lock
        var me = await _users.GetUserAsync(User);
        if (me?.Id == userId)
        {
            TempData["ToastOk"] = "You cannot lock your own account.";
            return RedirectToPage();
        }

        if (action == "lock")
        {
            await _users.SetLockoutEnabledAsync(user, true);
            await _users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(10));
            TempData["ToastOk"] = $"{user.Email} suspended.";
        }
        else
        {
            await _users.SetLockoutEndDateAsync(user, null);
            TempData["ToastOk"] = $"{user.Email} reactivated.";
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var me = await _users.GetUserAsync(User);
        if (me?.Id == userId)
        {
            TempData["ToastOk"] = "You cannot delete your own account.";
            return RedirectToPage();
        }

        var user = await _users.FindByIdAsync(userId);
        if (user == null) return NotFound();

        await _users.DeleteAsync(user);
        TempData["ToastOk"] = $"Account {user.Email} deleted.";
        return RedirectToPage();
    }

    // ─────────────────────────────────────────────────────────

    private async Task LoadStaffAsync()
    {
        var staffUsers = await _users.Users
            .Where(u => !u.IsPublicUser)
            .OrderBy(u => u.Email)
            .ToListAsync();

        foreach (var u in staffUsers)
        {
            var roles = await _users.GetRolesAsync(u);
            var isLocked = u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow;
            Staff.Add(new StaffRow
            {
                Id = u.Id,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Roles = roles.ToList(),
                IsLockedOut = isLocked
            });
        }
    }
}
