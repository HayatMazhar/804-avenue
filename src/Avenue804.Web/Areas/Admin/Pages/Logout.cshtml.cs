using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminAccess")]
public class LogoutModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Index", new { area = "Admin" });

    public async Task<IActionResult> OnPostAsync([FromServices] SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return RedirectToPage("/Index", new { area = "" });
    }
}
