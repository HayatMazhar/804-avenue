using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class OurServicesModel : PageModel
{
    public void OnGet()
    {
        ViewData["NavActive"] = "services";
    }
}
