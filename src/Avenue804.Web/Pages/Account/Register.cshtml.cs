using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly IEmailSender _email;

    public RegisterModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, IEmailSender email)
    {
        _users = users;
        _signIn = signIn;
        _email = email;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, StringLength(200)]
        [Display(Name = "Full name")]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl ?? "/";

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= "/";
        if (!ModelState.IsValid) return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Email.Trim(),
            Email = Input.Email.Trim(),
            DisplayName = Input.DisplayName.Trim(),
            IsPublicUser = true
        };

        var result = await _users.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return Page();
        }

        // Send welcome + verify email
        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        var link = Url.Page("/Account/VerifyEmail",
            pageHandler: null,
            values: new { userId = user.Id, token },
            protocol: Request.Scheme);

        await _email.SendAsync(user.Email!, "Verify your email — 804 Avenue",
            $"<p>Welcome to 804 Avenue!</p><p><a href='{link}'>Click here to verify your email address</a></p><p>Link valid for 24 hours.</p>");

        await _signIn.SignInAsync(user, isPersistent: true);
        TempData["ToastOk"] = "Account created! Please verify your email.";
        return LocalRedirect(returnUrl);
    }
}
