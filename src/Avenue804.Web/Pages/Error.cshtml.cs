using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public int? HttpStatusCode { get; set; }

    public void OnGet(int? code = null)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        HttpStatusCode = code;
    }
}
