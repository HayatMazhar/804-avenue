using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Avenue804.Web.Pages.Submit;

[EnableRateLimiting("submit")]
public class AmcRequestModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;
    private readonly IRecaptchaVerifier _recaptcha;

    public AmcRequestModel(ApplicationDbContext db, IEmailSender email, IConfiguration cfg, IFeatureFlagService flags, IRecaptchaVerifier recaptcha)
    {
        _db = db;
        _email = email;
        _cfg = cfg;
        _flags = flags;
        _recaptcha = recaptcha;
    }

    public IActionResult OnGet() => RedirectToPage("/Maintenance");

    public async Task<IActionResult> OnPostAsync(
        [Required, StringLength(200)] string name,
        [Required, StringLength(50)] string phone,
        [Required, EmailAddress, StringLength(256)] string email,
        [StringLength(200)] string? company,
        BuildingType buildingType,
        int? numberOfFloors,
        int? numberOfUnits,
        decimal? totalAreaSqm,
        [StringLength(400)] string? location,
        string? servicesNeeded,
        [StringLength(100)] string? estimateRange,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please complete all required fields.";
            return RedirectToPage("/Maintenance");
        }

        var rcToken = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptcha.VerifyAsync(rcToken, "amc_request", ct))
        {
            TempData["ToastError"] = "We couldn't verify your request. Please refresh the page and try again.";
            return RedirectToPage("/Maintenance");
        }

        var req = new AmcRequest
        {
            Name = name.Trim(),
            Phone = phone.Trim(),
            Email = email.Trim(),
            Company = company?.Trim(),
            BuildingType = buildingType,
            NumberOfFloors = numberOfFloors,
            NumberOfUnits = numberOfUnits,
            TotalAreaSqm = totalAreaSqm,
            Location = location?.Trim(),
            ServicesNeeded = servicesNeeded,
            EstimateRange = estimateRange?.Trim()
        };

        _db.AmcRequests.Add(req);
        await _db.SaveChangesAsync(ct);

        if (await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, ct))
        {
            var adminEmail = _cfg["Site:Email"] ?? "info@804avenue.com";
            await _email.SendAsync(adminEmail,
                $"New AMC Request — {buildingType} building — {name}",
                $"""
                <h3>New Annual Maintenance Contract request</h3>
                <table>
                  <tr><td><strong>Name:</strong></td><td>{name}</td></tr>
                  <tr><td><strong>Email:</strong></td><td>{email}</td></tr>
                  <tr><td><strong>Phone:</strong></td><td>{phone}</td></tr>
                  <tr><td><strong>Company:</strong></td><td>{company ?? "—"}</td></tr>
                  <tr><td><strong>Building type:</strong></td><td>{buildingType}</td></tr>
                  <tr><td><strong>Floors:</strong></td><td>{numberOfFloors?.ToString() ?? "—"}</td></tr>
                  <tr><td><strong>Units:</strong></td><td>{numberOfUnits?.ToString() ?? "—"}</td></tr>
                  <tr><td><strong>Total area:</strong></td><td>{(totalAreaSqm.HasValue ? totalAreaSqm + " sqm" : "—")}</td></tr>
                  <tr><td><strong>Location:</strong></td><td>{location ?? "—"}</td></tr>
                  <tr><td><strong>Services needed:</strong></td><td>{servicesNeeded ?? "—"}</td></tr>
                  <tr><td><strong>Estimate shown:</strong></td><td>{estimateRange ?? "—"}</td></tr>
                </table>
                <p><a href="/Admin/ServiceRequests?tab=1">View in admin</a></p>
                """, ct);
        }

        TempData["ToastOk"] = "AMC request received! We will prepare a customised proposal within 48 hours.";
        return RedirectToPage("/Maintenance");
    }
}
