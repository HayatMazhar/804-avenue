using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IEmailSender _email;

    public ForgotPasswordModel(UserManager<ApplicationUser> users, IEmailSender email)
    {
        _users = users;
        _email = email;
    }

    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
    public bool Sent { get; private set; }
    public string? Error { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var user = await _users.FindByEmailAsync(Email.Trim());
        if (user != null)
        {
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var link = Url.Page("/Account/ResetPassword",
                pageHandler: null,
                values: new { userId = user.Id, token },
                protocol: Request.Scheme);

            await _email.SendAsync(Email.Trim(), "Reset your 804 Avenue password",
                $"<p>Click the link below to reset your password:</p><p><a href='{link}'>Reset Password</a></p><p>This link expires in 24 hours.</p>");
        }

        Sent = true;
        return Page();
    }
}
