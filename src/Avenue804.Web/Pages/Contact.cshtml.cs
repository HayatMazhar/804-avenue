using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class ContactModel : PageModel
{
    private readonly IInquirySubmitter _inquiries;

    public ContactModel(IInquirySubmitter inquiries) => _inquiries = inquiries;

    [BindProperty]
    public InquiryFormModel Form { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please correct the highlighted fields and try again.";
            return Page();
        }

        await _inquiries.SubmitAsync(Form, cancellationToken);
        TempData["ToastOk"] = "Thank you — your message was sent. Our team will reply soon.";
        return RedirectToPage();
    }
}
