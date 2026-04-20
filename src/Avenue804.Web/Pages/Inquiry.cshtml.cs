using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

// Property-requirement inquiry form. Used to be on /Contact, now lives on its
// own dedicated /Inquiry page so the public Contact page can stay a plain
// "get in touch" form.
public class InquiryPageModel : PageModel
{
    private readonly IInquirySubmitter _inquiries;

    public InquiryPageModel(IInquirySubmitter inquiries) => _inquiries = inquiries;

    // ── Contact details ──────────────────────────────────────────
    [BindProperty, Required(ErrorMessage = "Please enter your name."), StringLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Please enter your mobile number."), StringLength(50)]
    [Display(Name = "Mobile Number")]
    public string Phone { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Please enter your email address."), EmailAddress, StringLength(256)]
    [Display(Name = "Email ID")]
    public string Email { get; set; } = string.Empty;

    // ── Property requirement ─────────────────────────────────────
    [BindProperty, Required(ErrorMessage = "Please choose what you want to do."), StringLength(10)]
    [Display(Name = "I Want to")]
    public string WantTo { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Please choose a property type."), StringLength(20)]
    [Display(Name = "Property Type")]
    public string PropertyCategory { get; set; } = string.Empty;

    [BindProperty, Required(ErrorMessage = "Please choose property details."), StringLength(80)]
    [Display(Name = "Property Details")]
    public string PropertyDetails { get; set; } = string.Empty;

    [BindProperty, StringLength(200)]
    [Display(Name = "If Others")]
    public string? IfOthers { get; set; }

    [BindProperty, StringLength(4000)]
    [Display(Name = "Requirement Details")]
    public string? RequirementDetails { get; set; }

    [BindProperty, StringLength(200)]
    [Display(Name = "Area")]
    public string? Area { get; set; }

    [BindProperty, StringLength(80)]
    [Display(Name = "Budget")]
    public string? Budget { get; set; }

    [BindProperty, DataType(DataType.Date)]
    [Display(Name = "Expected Move In Date")]
    public DateTime? MoveInDate { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please correct the highlighted fields and try again.";
            return Page();
        }

        var details = string.Equals(PropertyDetails, "Others", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(IfOthers)
            ? $"Others — {IfOthers!.Trim()}"
            : PropertyDetails;

        var subjectParts = new List<string> { WantTo, PropertyCategory, details };
        if (!string.IsNullOrWhiteSpace(Area)) subjectParts.Add($"in {Area.Trim()}");
        var subject = "Property requirement — " + string.Join(" · ", subjectParts);

        var lines = new List<string>
        {
            $"— Looking to: {WantTo}",
            $"— Property type: {PropertyCategory}",
            $"— Property details: {details}",
        };
        if (!string.IsNullOrWhiteSpace(Area))   lines.Add($"— Area: {Area.Trim()}");
        if (!string.IsNullOrWhiteSpace(Budget)) lines.Add($"— Budget / range: {Budget}");
        if (MoveInDate.HasValue)                lines.Add($"— Expected move-in: {MoveInDate.Value:yyyy-MM-dd}");
        lines.Add(string.Empty);
        lines.Add(string.IsNullOrWhiteSpace(RequirementDetails)
            ? "(No additional requirement details provided.)"
            : RequirementDetails!.Trim());

        var message = string.Join("\n", lines);

        await _inquiries.SubmitAsync(new InquiryFormModel
        {
            Name = Name.Trim(),
            Email = Email.Trim(),
            Phone = Phone.Trim(),
            Subject = subject,
            Message = message
        }, cancellationToken);

        TempData["ToastOk"] = "Thank you — we will contact you shortly for further details.";
        return RedirectToPage();
    }
}
