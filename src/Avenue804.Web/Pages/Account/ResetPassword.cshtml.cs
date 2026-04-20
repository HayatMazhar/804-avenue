using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;

    public ResetPasswordModel(UserManager<ApplicationUser> users) => _users = users;

    [BindProperty] public string UserId { get; set; } = string.Empty;
    [BindProperty] public string Token { get; set; } = string.Empty;
    [BindProperty, Required, StringLength(100, MinimumLength = 8), DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    [BindProperty, Required, DataType(DataType.Password), Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool Done { get; private set; }

    public void OnGet(string userId, string token) { UserId = userId; Token = token; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = await _users.FindByIdAsync(UserId);
        if (user == null) { ModelState.AddModelError("", "Invalid reset link."); return Page(); }
        var result = await _users.ResetPasswordAsync(user, Token, Password);
        if (!result.Succeeded) { foreach (var e in result.Errors) ModelState.AddModelError("", e.Description); return Page(); }
        Done = true;
        return Page();
    }
}
