using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Newsletter;

[Authorize(Policy = "AdminOnly")]
public class DigestModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;

    public DigestModel(ApplicationDbContext db, IEmailSender email, IConfiguration cfg)
    {
        _db = db; _email = email; _cfg = cfg;
    }

    public string? Result { get; private set; }
    public bool Success { get; private set; }

    public void OnGet() { ViewData["AdminSection"] = "newsletter"; }

    public async Task<IActionResult> OnPostAsync(bool testOnly, CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "newsletter";

        var siteUrl = _cfg["Site:WebsiteUrl"]?.TrimEnd('/') ?? "https://www.804avenue.ae";
        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);
        var newListings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.CreatedAt >= cutoff)
            .OrderByDescending(p => p.ViewCount).Take(12).ToListAsync(ct);

        if (!newListings.Any())
        {
            Result = "No new listings in the past 7 days — nothing to send.";
            return Page();
        }

        var htmlListings = BuildListingsHtml(newListings, siteUrl);

        if (testOnly)
        {
            var adminEmail = _cfg["Site:Email"] ?? _cfg["Seed:AdminEmail"] ?? "info@804avenue.com";
            await _email.SendAsync(adminEmail, "[TEST] Weekly Digest — 804 Avenue", BuildDigestEmail("Admin (test)", newListings.Count, htmlListings, siteUrl), ct);
            Result = $"Test digest sent to {adminEmail} featuring {newListings.Count} listings.";
        }
        else
        {
            int sent = 0;
            var subscribers = await _db.NewsletterSubscribers.Where(s => s.IsActive).ToListAsync(ct);
            foreach (var s in subscribers)
            {
                await _email.SendAsync(s.Email, "New listings this week — 804 Avenue", BuildDigestEmail(s.Name, newListings.Count, htmlListings, siteUrl), ct);
                sent++;
            }
            Result = $"Digest sent to {sent} subscribers featuring {newListings.Count} new listings.";
        }

        Success = true;
        return Page();
    }

    private static string BuildListingsHtml(List<PropertyListing> listings, string siteUrl)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var p in listings)
        {
            var price = p.Price.HasValue ? $"AED {p.Price.Value:N0}" : "Price on request";
            var img = !string.IsNullOrWhiteSpace(p.MainImageUrl) ? p.MainImageUrl : "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=600&q=80";
            sb.AppendLine($"<tr><td style=\"padding:10px 0;border-bottom:1px solid #e2e8f0\"><strong><a href=\"{siteUrl}/Properties/{p.Slug}\">{p.Title}</a></strong> — {price} — {p.Location}</td></tr>");
        }
        return sb.ToString();
    }

    private static string BuildDigestEmail(string? name, int count, string html, string siteUrl) =>
        $"<p>Hi {name ?? "there"},</p><p><strong>{count} new listings</strong> added this week:</p><table>{html}</table><p><a href=\"{siteUrl}/Properties\">Browse all properties</a></p>";
}
