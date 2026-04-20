using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return Page();

        var user = await _userManager.FindByEmailAsync(Input.Email.Trim());
        if (user == null)
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        // Verify the user is an admin (has the Admin role) before signing in
        if (!await _userManager.IsInRoleAsync(user, SeedData.AdminRole))
        {
            ErrorMessage = "You do not have admin access.";
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(user, Input.Password, isPersistent: true, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            ErrorMessage = result.IsLockedOut ? "Account locked. Try again later." : "Invalid email or password.";
            return Page();
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);
        return RedirectToPage("/Index", new { area = "Admin" });
    }
}
