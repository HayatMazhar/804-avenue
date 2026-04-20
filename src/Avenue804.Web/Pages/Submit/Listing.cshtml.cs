using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Forms;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Avenue804.Web.Pages.Submit;

[EnableRateLimiting("submit")]
public class ListingModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;

    public ListingModel(ApplicationDbContext db, IEmailSender email, IConfiguration cfg, IFeatureFlagService flags)
    {
        _db = db;
        _email = email;
        _cfg = cfg;
        _flags = flags;
    }

    public IActionResult OnGet() => RedirectToPage("/Index");

    public async Task<IActionResult> OnPostAsync(
        OwnerListingFormModel input,
        string? lpWhatsapp, string? lpPropertyType, string? lpEmirate, string? lpArea,
        string? lpBeds, string? lpBaths, string? lpSize, string? lpPrice,
        string? lpCondition, string? lpAvailable, string? lpBestTime,
        string? lpIAm, string? lpPropertyCategory, string? lpPropertyDetails, string? lpIfOthers,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        // Resolve a safe local return URL — only allow same-site relative paths.
        IActionResult RedirectBack(string? toast = null, bool error = false)
        {
            if (toast != null) TempData[error ? "ToastError" : "ToastOk"] = toast;
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);
            return RedirectToPage("/Index");
        }

        if (!ModelState.IsValid)
        {
            return RedirectBack("Please check the listing form and try again.", error: true);
        }

        var extras = new List<string>();
        if (!string.IsNullOrWhiteSpace(lpIAm)) extras.Add($"I am: {lpIAm}");
        if (!string.IsNullOrWhiteSpace(lpWhatsapp)) extras.Add($"WhatsApp: {lpWhatsapp}");
        if (!string.IsNullOrWhiteSpace(lpPropertyCategory)) extras.Add($"Property type: {lpPropertyCategory}");
        if (!string.IsNullOrWhiteSpace(lpPropertyDetails)) extras.Add($"Property details: {lpPropertyDetails}");
        if (!string.IsNullOrWhiteSpace(lpIfOthers)) extras.Add($"Other / specify: {lpIfOthers}");
        if (!string.IsNullOrWhiteSpace(lpPropertyType)) extras.Add($"Type: {lpPropertyType}");
        if (!string.IsNullOrWhiteSpace(lpEmirate)) extras.Add($"Emirate: {lpEmirate}");
        if (!string.IsNullOrWhiteSpace(lpArea)) extras.Add($"Area: {lpArea}");
        if (!string.IsNullOrWhiteSpace(lpBeds)) extras.Add($"Beds: {lpBeds}");
        if (!string.IsNullOrWhiteSpace(lpBaths)) extras.Add($"Baths: {lpBaths}");
        if (!string.IsNullOrWhiteSpace(lpSize)) extras.Add($"Size: {lpSize} sqft");
        if (!string.IsNullOrWhiteSpace(lpPrice)) extras.Add($"Price: AED {lpPrice}");
        if (!string.IsNullOrWhiteSpace(lpCondition)) extras.Add($"Condition: {lpCondition}");
        if (!string.IsNullOrWhiteSpace(lpAvailable)) extras.Add($"Available: {lpAvailable}");
        if (!string.IsNullOrWhiteSpace(lpBestTime)) extras.Add($"Best time: {lpBestTime}");

        var baseDetails = string.IsNullOrWhiteSpace(input.Details) ? "" : input.Details.Trim();
        var details = extras.Count > 0
            ? (baseDetails + (baseDetails.Length > 0 ? "\n" : "") + string.Join("\n", extras)).Trim()
            : (string.IsNullOrWhiteSpace(baseDetails) ? null : baseDetails);

        _db.OwnerListingRequests.Add(new OwnerListingRequest
        {
            Name = input.Name.Trim(),
            Email = input.Email.Trim(),
            Phone = input.Phone.Trim(),
            Intent = input.Intent,
            LocationOrTitle = string.IsNullOrWhiteSpace(input.LocationOrTitle) ? null : input.LocationOrTitle.Trim(),
            Details = details
        });

        await _db.SaveChangesAsync(cancellationToken);

        if (await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, cancellationToken))
        {
        // Notify admin
        var adminEmail = _cfg["Site:Email"] ?? _cfg["Seed:AdminEmail"] ?? "info@804avenue.com";
        var intentLabel = input.Intent switch { OwnerListingIntent.Sale => "Sell", OwnerListingIntent.Rent => "Rent", OwnerListingIntent.Both => "Sell & Rent", _ => "Unknown" };
        await _email.SendAsync(adminEmail,
            $"New Listing Request from {input.Name}",
            $"""
            <h3>New property listing request</h3>
            <table>
              <tr><td><strong>Name:</strong></td><td>{input.Name}</td></tr>
              <tr><td><strong>Email:</strong></td><td>{input.Email}</td></tr>
              <tr><td><strong>Phone:</strong></td><td>{input.Phone}</td></tr>
              <tr><td><strong>Intent:</strong></td><td>{intentLabel}</td></tr>
              <tr><td><strong>Location / Title:</strong></td><td>{input.LocationOrTitle ?? "—"}</td></tr>
              <tr><td><strong>Details:</strong></td><td>{details ?? "—"}</td></tr>
            </table>
            <p><a href="/Admin/OwnerRequests">View in admin</a></p>
            """,
            cancellationToken);
        } // end email notification check

        return RedirectBack("Thank you — we received your listing request. We will contact you shortly for further details.");
    }
}
