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
    private readonly ILogger<PropertyListingInquiryModel> _log;

    public PropertyListingInquiryModel(IPropertyListingInquirySubmitter submitter, IRecaptchaVerifier recaptcha, ILogger<PropertyListingInquiryModel> log)
    {
        _submitter = submitter;
        _recaptcha = recaptcha;
        _log = log;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(PropertyListingInquiryFormModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState
                .Where(kv => kv.Value?.Errors.Count > 0)
                .Select(kv => $"{kv.Key}: {string.Join(",", kv.Value!.Errors.Select(e => e.ErrorMessage))}"));
            _log.LogWarning("Property inquiry rejected — invalid model state: {Errors}", errors);
            TempData["ToastError"] = "Please fill in all required fields and try again.";
            return RedirectToPage("/Index");
        }

        var rcToken = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptcha.VerifyAsync(rcToken, "property_inquiry", cancellationToken))
        {
            _log.LogWarning("Property inquiry rejected — reCAPTCHA verification failed (token present: {HasToken}).", !string.IsNullOrEmpty(rcToken));
            TempData["ToastError"] = "We couldn't verify your request. Please refresh the page and try again.";
            return RedirectToPage("/Index");
        }

        if (!await _submitter.TrySubmitAsync(input, cancellationToken))
        {
            _log.LogWarning("Property inquiry rejected — submitter returned false (lookup resolution). WantTo={WantTo}, Type={Type}, Budget={Budget}, Detail={Detail}",
                input.WantToLookupValueId, input.PropertyTypeLookupValueId, input.BudgetLookupValueId, input.PropertyDetailLookupValueId);
            TempData["ToastError"] = "Something went wrong with your selections. Please try again.";
            return RedirectToPage("/Index");
        }

        TempData["ToastOk"] = "We will contact you shortly for further details.";
        return RedirectToPage("/Index");
    }
}
