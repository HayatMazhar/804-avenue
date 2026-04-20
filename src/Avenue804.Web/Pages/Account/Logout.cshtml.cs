using Avenue804.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signIn;

    public LogoutModel(SignInManager<ApplicationUser> signIn) => _signIn = signIn;

    public async Task<IActionResult> OnGetAsync()
    {
        await _signIn.SignOutAsync();
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _signIn.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
