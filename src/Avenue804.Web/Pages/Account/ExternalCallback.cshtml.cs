using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Avenue804.Web.Pages.Account;

[AllowAnonymous]
public class ExternalCallbackModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser> _users;

    public ExternalCallbackModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    {
        _signIn = signIn;
        _users = users;
    }

    public async Task<IActionResult> OnGetAsync(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= "/";

        if (remoteError != null)
        {
            TempData["ToastError"] = $"Social sign-in error: {remoteError}";
            return RedirectToPage("/Account/Login");
        }

        var info = await _signIn.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ToastError"] = "Could not retrieve login information. Please try again.";
            return RedirectToPage("/Account/Login");
        }

        var result = await _signIn.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        // First time — create user
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? info.Principal.FindFirstValue("name") ?? email ?? "";
        var avatar = info.Principal.FindFirstValue("picture") ?? info.Principal.FindFirstValue("avatar");

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["ToastError"] = "No email address was returned by the provider. Please use email registration.";
            return RedirectToPage("/Account/Register");
        }

        var user = await _users.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = name,
                AvatarUrl = avatar,
                IsPublicUser = true
            };
            var createResult = await _users.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                TempData["ToastError"] = "Account creation failed. Please try again.";
                return RedirectToPage("/Account/Register");
            }
        }

        await _users.AddLoginAsync(user, info);
        await _signIn.SignInAsync(user, isPersistent: true);
        TempData["ToastOk"] = $"Welcome, {user.DisplayName ?? user.Email}!";
        return LocalRedirect(returnUrl);
    }
}
