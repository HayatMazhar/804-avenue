using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace Avenue804.Web.Pages.Submit;

[EnableRateLimiting("submit")]
public class MaintenanceRequestModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;
    private readonly IAdminNotificationService _notif;
    private readonly IRecaptchaVerifier _recaptcha;

    public MaintenanceRequestModel(ApplicationDbContext db, IEmailSender email, IConfiguration cfg, IFeatureFlagService flags, IAdminNotificationService notif, IRecaptchaVerifier recaptcha)
    {
        _db = db; _email = email; _cfg = cfg; _flags = flags; _notif = notif; _recaptcha = recaptcha;
    }

    public IActionResult OnGet() => RedirectToPage("/Maintenance");

    public async Task<IActionResult> OnPostAsync(
        [Required, StringLength(200)] string name,
        [Required, StringLength(50)] string phone,
        [Required, EmailAddress, StringLength(256)] string email,
        [Required, StringLength(400)] string buildingOrLocation,
        [Required, StringLength(4000)] string issueDescription,
        TicketPriority priority = TicketPriority.Normal,
        CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["ToastError"] = "Please fill in all required fields.";
            return RedirectToPage("/Maintenance");
        }

        var rcToken = Request.Form["g-recaptcha-response"].ToString();
        if (!await _recaptcha.VerifyAsync(rcToken, "maintenance_request", ct))
        {
            TempData["ToastError"] = "We couldn't verify your request. Please refresh the page and try again.";
            return RedirectToPage("/Maintenance");
        }

        var ticket = new MaintenanceTicket
        {
            Name = name.Trim(),
            Phone = phone.Trim(),
            Email = email.Trim(),
            BuildingOrLocation = buildingOrLocation.Trim(),
            IssueDescription = issueDescription.Trim(),
            Priority = priority
        };

        _db.MaintenanceTickets.Add(ticket);
        await _db.SaveChangesAsync(ct);

        // Always create admin notification (no email flag needed for in-app)
        var notifMsg = priority == TicketPriority.Emergency
            ? $"🚨 EMERGENCY from {name}: {buildingOrLocation}"
            : $"New maintenance ticket from {name}: {buildingOrLocation}";
        await _notif.CreateAsync(
            priority == TicketPriority.Emergency ? Domain.NotificationType.NewEmergencyTicket : Domain.NotificationType.NewInquiry,
            notifMsg, "/Admin/ServiceRequests?tab=2", ct);

        if (await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, ct))
        {
            var adminEmail = _cfg["Site:Email"] ?? "info@804avenue.ae";
            var priorityLabel = priority == TicketPriority.Emergency ? "🚨 EMERGENCY" : priority == TicketPriority.High ? "⚠ HIGH" : "Normal";
            await _email.SendAsync(adminEmail,
                $"[{priorityLabel}] Maintenance Request — {name} — {buildingOrLocation}",
                $"""
                <h3 style="color:{(priority == TicketPriority.Emergency ? "#DC2626" : "#000")}">Maintenance ticket #{ticket.Id} — {priorityLabel}</h3>
                <table>
                  <tr><td><strong>Name:</strong></td><td>{name}</td></tr>
                  <tr><td><strong>Phone:</strong></td><td>{phone}</td></tr>
                  <tr><td><strong>Email:</strong></td><td>{email}</td></tr>
                  <tr><td><strong>Building/Location:</strong></td><td>{buildingOrLocation}</td></tr>
                  <tr><td><strong>Issue:</strong></td><td>{issueDescription}</td></tr>
                </table>
                <p><a href="/Admin/ServiceRequests?tab=2">View in admin</a></p>
                """, ct);
        }

        var msg = priority == TicketPriority.Emergency
            ? "🚨 Emergency request received! Our team will contact you within 30 minutes."
            : "Maintenance request submitted. We will schedule your service visit within 24 hours.";
        TempData["ToastOk"] = msg;
        return RedirectToPage("/Maintenance");
    }
}
