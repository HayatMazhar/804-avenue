using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Communities;

public class CommunitiesIndexModel : PageModel
{
    public IActionResult OnGet(string? q = null, string? emirate = null)
        => RedirectToPagePermanent("/AreaGuides/Index", new { q, emirate });
}
