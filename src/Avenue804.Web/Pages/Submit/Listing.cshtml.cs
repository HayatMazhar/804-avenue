using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Submit;

public class ListingModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public ListingModel(ApplicationDbContext db) => _db = db;

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(OwnerListingFormModel input, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please check the listing form and try again.";
            return RedirectToPage("/Index");
        }

        _db.OwnerListingRequests.Add(new OwnerListingRequest
        {
            Name = input.Name.Trim(),
            Email = input.Email.Trim(),
            Phone = input.Phone.Trim(),
            Intent = input.Intent,
            LocationOrTitle = string.IsNullOrWhiteSpace(input.LocationOrTitle) ? null : input.LocationOrTitle.Trim(),
            Details = string.IsNullOrWhiteSpace(input.Details) ? null : input.Details.Trim()
        });

        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Thank you — we received your listing request and will contact you shortly.";
        return RedirectToPage("/Index");
    }
}
