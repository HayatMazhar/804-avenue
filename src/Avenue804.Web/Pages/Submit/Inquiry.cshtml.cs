using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Avenue804.Web.Pages.Submit;

[EnableRateLimiting("submit")]
public class InquiryModel : PageModel
{
    private readonly IInquirySubmitter _inquiries;

    public InquiryModel(IInquirySubmitter inquiries) => _inquiries = inquiries;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(InquiryFormModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please check the inquiry form and try again.";
            return RedirectToPage("/Index");
        }

        await _inquiries.SubmitAsync(input, cancellationToken);
        TempData["ToastOk"] = "Thank you — your inquiry was sent. Our team will reply soon.";
        return RedirectToPage("/Index");
    }
}
