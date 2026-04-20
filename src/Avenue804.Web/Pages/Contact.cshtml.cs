using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

// Plain "get in touch" contact form. The richer property-requirement form
// lives on /Inquiry — keep this one intentionally simple.
public class ContactModel : PageModel
{
    private readonly IInquirySubmitter _inquiries;

    public ContactModel(IInquirySubmitter inquiries) => _inquiries = inquiries;

    [BindProperty, Required(ErrorMessage = "Please enter your name."), StringLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Please enter your email address."), EmailAddress, StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [BindProperty, StringLength(50)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [BindProperty, StringLength(200)]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [BindProperty, Required(ErrorMessage = "Please enter your message."), StringLength(4000, MinimumLength = 5, ErrorMessage = "Please write at least a few words about how we can help.")]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please correct the highlighted fields and try again.";
            return Page();
        }

        await _inquiries.SubmitAsync(new InquiryFormModel
        {
            Name = Name.Trim(),
            Email = Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(Phone) ? null : Phone!.Trim(),
            Subject = string.IsNullOrWhiteSpace(Subject) ? "Contact form message" : Subject!.Trim(),
            Message = Message.Trim()
        }, cancellationToken);

        TempData["ToastOk"] = "Thank you — your message has been received. We will get back to you shortly.";
        return RedirectToPage();
    }
}
