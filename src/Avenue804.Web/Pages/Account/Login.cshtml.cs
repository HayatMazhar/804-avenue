using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Avenue804.Web.Pages.Account;

[AllowAnonymous]
[EnableRateLimiting("login")]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;
    private readonly ITwilioOtpService _otp;

    public LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users, ITwilioOtpService otp)
    {
        _signIn = signIn;
        _users = users;
        _otp = otp;
    }

    [BindProperty, EmailAddress] public string Email { get; set; } = string.Empty;
    [BindProperty, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [BindProperty] public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PhoneError { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl ?? "/";

    public async Task<IActionResult> OnPostEmailAsync(string? returnUrl = null)
    {
        returnUrl ??= "/";
        if (!ModelState.IsValid) return Page();

        var user = await _users.FindByEmailAsync(Email.Trim());
        if (user == null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        var result = await _signIn.PasswordSignInAsync(user, Password, RememberMe, lockoutOnFailure: true);
        if (result.IsLockedOut) { ErrorMessage = "Account locked. Try again later."; return Page(); }
        if (!result.Succeeded) { ErrorMessage = "Invalid email or password."; return Page(); }

        return LocalRedirect(returnUrl);
    }

    [EnableRateLimiting("otp-send")]
    public async Task<IActionResult> OnPostSendOtpAsync(string phoneNumber, string? returnUrl = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            PhoneError = "Please enter a valid phone number.";
            return Page();
        }
        await _otp.SendOtpAsync(phoneNumber.Trim());
        TempData["OtpPhone"] = phoneNumber.Trim();
        return Page();
    }

    public async Task<IActionResult> OnPostVerifyOtpAsync(string phoneNumber, string code, string? returnUrl = null)
    {
        returnUrl ??= "/";
        if (!_otp.VerifyOtp(phoneNumber, code))
        {
            PhoneError = "Invalid or expired code. Please try again.";
            return Page();
        }

        // Find or create user by phone
        var normalised = phoneNumber.Trim();
        var user = await _users.FindByNameAsync(normalised)
                   ?? _users.Users.FirstOrDefault(u => u.PhoneNumber == normalised);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = normalised,
                PhoneNumber = normalised,
                PhoneNumberConfirmed = true,
                PhoneVerified = true,
                IsPublicUser = true,
                DisplayName = normalised
            };
            await _users.CreateAsync(user);
        }

        await _signIn.SignInAsync(user, isPersistent: true);
        return LocalRedirect(returnUrl);
    }
}
