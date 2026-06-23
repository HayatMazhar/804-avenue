using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Communities;

public class CommunitiesDetailModel : PageModel
{
    public IActionResult OnGet(string slug)
        => RedirectToPagePermanent("/AreaGuides/Detail", new { slug });
}
