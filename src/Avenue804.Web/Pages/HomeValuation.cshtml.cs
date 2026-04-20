using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class HomeValuationModel : PageModel
{
    public void OnGet() { ViewData["Title"] = "Free Home Valuation"; }
}
