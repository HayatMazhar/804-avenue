using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.ListingRequests;

/// <summary>
/// Compatibility shim — the real page lives at /Admin/OwnerRequests.
/// "Listing requests" is the user-facing label, so anyone bookmarking
/// /Admin/ListingRequests gets redirected to the correct URL.
/// </summary>
[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPagePermanent("/OwnerRequests/Index", new { area = "Admin" });
}
