using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Account;

[AllowAnonymous]
public class VerifyEmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;

    public VerifyEmailModel(UserManager<ApplicationUser> users) => _users = users;

    public bool Success { get; private set; }
    public string? Error { get; private set; }

    public async Task OnGetAsync(string userId, string token)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user == null) { Error = "Invalid verification link."; return; }
        var result = await _users.ConfirmEmailAsync(user, token);
        if (result.Succeeded) Success = true;
        else Error = "Verification link is invalid or has expired.";
    }
}
