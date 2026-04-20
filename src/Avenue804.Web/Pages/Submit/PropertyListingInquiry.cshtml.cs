using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Avenue804.Web.Pages.Submit;

[EnableRateLimiting("submit")]
public class PropertyListingInquiryModel : PageModel
{
    private readonly IPropertyListingInquirySubmitter _submitter;
    private readonly IRecaptchaVerifier _recaptcha;

    public PropertyListingInquiryModel(IPropertyListingInquirySubmitter submitter, IRecaptchaVerifier recaptcha)
    {
        _submitter = submitter;
        _recaptcha = recaptcha;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(PropertyListingInquiryFormModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please fill in all required fields and try again.";
            return RedirectToPage("/Index");
        }

        var rcToken = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptcha.VerifyAsync(rcToken, "property_inquiry", cancellationToken))
        {
            TempData["ToastError"] = "We couldn't verify your request. Please refresh the page and try again.";
            return RedirectToPage("/Index");
        }

        if (!await _submitter.TrySubmitAsync(input, cancellationToken))
        {
            TempData["ToastError"] = "Something went wrong with your selections. Please try again.";
            return RedirectToPage("/Index");
        }

        TempData["ToastOk"] = "We will contact you shortly for further details.";
        return RedirectToPage("/Index");
    }
}
