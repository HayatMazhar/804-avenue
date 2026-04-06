using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Submit;

public class PropertyListingInquiryModel : PageModel
{
    private readonly IPropertyListingInquirySubmitter _submitter;

    public PropertyListingInquiryModel(IPropertyListingInquirySubmitter submitter) => _submitter = submitter;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(PropertyListingInquiryFormModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please fill in all required fields and try again.";
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
