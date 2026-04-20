using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Account;

public class ProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;

    public ProfileModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    {
        _users = users;
        _signIn = signIn;
    }

    [BindProperty, StringLength(200)] public string? DisplayName { get; set; }
    [BindProperty, StringLength(2000), Url] public string? AvatarUrl { get; set; }
    [BindProperty, StringLength(500)] public string? Bio { get; set; }
    [BindProperty, DataType(DataType.Password)] public string? CurrentPassword { get; set; }
    [BindProperty, StringLength(100, MinimumLength = 8), DataType(DataType.Password)] public string? NewPassword { get; set; }
    [BindProperty, DataType(DataType.Password), Compare("NewPassword")] public string? ConfirmNewPassword { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");
        DisplayName = user.DisplayName;
        AvatarUrl = user.AvatarUrl;
        Bio = user.Bio;
        return Page();
    }

    public async Task<IActionResult> OnPostProfileAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");
        if (!ModelState.IsValid) return Page();
        user.DisplayName = DisplayName?.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(AvatarUrl) ? null : AvatarUrl.Trim();
        user.Bio = string.IsNullOrWhiteSpace(Bio) ? null : Bio.Trim();
        await _users.UpdateAsync(user);
        TempData["ProfileOk"] = "Profile updated successfully.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostPasswordAsync()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ModelState.AddModelError("", "All password fields are required.");
            return Page();
        }
        var result = await _users.ChangePasswordAsync(user, CurrentPassword, NewPassword);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            return Page();
        }
        await _signIn.RefreshSignInAsync(user);
        TempData["ProfileOk"] = "Password updated successfully.";
        return RedirectToPage();
    }
}
