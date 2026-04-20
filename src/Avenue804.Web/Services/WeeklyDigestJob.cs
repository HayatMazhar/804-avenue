using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Services;

/// <summary>
/// Runs every Sunday at 08:00 UTC. Emails all active newsletter subscribers and users with
/// saved searches, summarising new listings from the past 7 days.
/// Configure WeeklyDigest:DayOfWeek (Sunday default) and WeeklyDigest:HourUtc (8 default).
/// </summary>
public class WeeklyDigestJob : BackgroundService
{
    private readonly IServiceScopeFactory _scope;
    private readonly IConfiguration _cfg;
    private readonly ILogger<WeeklyDigestJob> _log;

    public WeeklyDigestJob(IServiceScopeFactory scope, IConfiguration cfg, ILogger<WeeklyDigestJob> log)
    {
        _scope = scope;
        _cfg = cfg;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _log.LogInformation("Weekly digest job started.");

        while (!ct.IsCancellationRequested)
        {
            var delay = GetDelayUntilNextRun();
            _log.LogInformation("Next weekly digest in {Hours:N1} hours.", delay.TotalHours);

            try { await Task.Delay(delay, ct); }
            catch (TaskCanceledException) { break; }

            await RunDigestAsync(ct);
        }
    }

    private TimeSpan GetDelayUntilNextRun()
    {
        var dayOfWeek = Enum.TryParse<DayOfWeek>(_cfg["WeeklyDigest:DayOfWeek"] ?? "Sunday", out var d) ? d : DayOfWeek.Sunday;
        var hourUtc = int.TryParse(_cfg["WeeklyDigest:HourUtc"], out var h) ? h : 8;

        var now = DateTimeOffset.UtcNow;
        var daysUntil = ((int)dayOfWeek - (int)now.DayOfWeek + 7) % 7;
        if (daysUntil == 0 && now.Hour >= hourUtc) daysUntil = 7;

        var nextRun = now.Date.AddDays(daysUntil).AddHours(hourUtc);
        var delay = nextRun - now;
        return delay < TimeSpan.Zero ? TimeSpan.FromMinutes(1) : delay;
    }

    private async Task RunDigestAsync(CancellationToken ct)
    {
        _log.LogInformation("Running weekly digest…");
        using var scope = _scope.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var flags = scope.ServiceProvider.GetRequiredService<IFeatureFlagService>();

        if (!await flags.IsEnabledAsync(FeatureFlags.EmailNotifications, ct))
        {
            _log.LogInformation("Email notifications disabled — skipping weekly digest.");
            return;
        }

        var cutoff = DateTimeOffset.UtcNow.AddDays(-7);
        var newListings = await db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.CreatedAt >= cutoff)
            .OrderByDescending(p => p.ViewCount)
            .Take(12)
            .ToListAsync(ct);

        if (!newListings.Any())
        {
            _log.LogInformation("No new listings in the past 7 days — skipping digest.");
            return;
        }

        var siteUrl = _cfg["Site:WebsiteUrl"]?.TrimEnd('/') ?? "https://www.804avenue.ae";
        var htmlListings = BuildListingsHtml(newListings, siteUrl);

        // Newsletter subscribers
        var subscribers = await db.NewsletterSubscribers
            .Where(s => s.IsActive && s.Email != null)
            .ToListAsync(ct);

        int sent = 0;
        foreach (var sub in subscribers)
        {
            await emailSender.SendAsync(sub.Email!,
                $"New listings this week — 804 Avenue",
                BuildDigestEmail(sub.Name, newListings.Count, htmlListings, siteUrl), ct);
            sent++;
        }

        // Registered users with saved searches (not already in newsletter)
        var newsletterEmails = subscribers.Select(s => s.Email!.ToLower()).ToHashSet();
        var savedSearchUsers = await db.SavedSearches
            .Include(s => s.User)
            .Where(s => s.AlertEnabled && s.User.Email != null && s.User.IsPublicUser)
            .GroupBy(s => s.UserId)
            .Select(g => g.First().User)
            .ToListAsync(ct);

        foreach (var u in savedSearchUsers)
        {
            if (string.IsNullOrWhiteSpace(u.Email) || newsletterEmails.Contains(u.Email.ToLower())) continue;
            await emailSender.SendAsync(u.Email,
                $"New properties this week — 804 Avenue",
                BuildDigestEmail(u.DisplayName ?? u.Email, newListings.Count, htmlListings, siteUrl), ct);
            sent++;
        }

        _log.LogInformation("Weekly digest sent to {Count} recipients. {Listings} new listings featured.", sent, newListings.Count);
    }

    private static string BuildListingsHtml(List<PropertyListing> listings, string siteUrl)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var p in listings)
        {
            var price = p.Price.HasValue ? $"AED {p.Price.Value:N0}{(p.OfferType == ListingOfferType.Rent ? "/yr" : "")}" : "Price on request";
            var img = !string.IsNullOrWhiteSpace(p.MainImageUrl) ? p.MainImageUrl : "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=600&q=80";
            sb.AppendLine($"""
                <tr>
                  <td style="padding:12px 0;border-bottom:1px solid #e2e8f0">
                    <table width="100%"><tr>
                      <td width="120"><img src="{img}" width="120" height="80" style="border-radius:8px;object-fit:cover" alt="{p.Title}" /></td>
                      <td style="padding-left:14px;vertical-align:top">
                        <a href="{siteUrl}/Properties/{p.Slug}" style="font-family:Georgia,serif;font-size:15px;font-weight:700;color:#0f172a;text-decoration:none">{p.Title}</a>
                        <p style="margin:4px 0;font-size:13px;color:#64748b">{(p.Location ?? "Abu Dhabi, UAE")}</p>
                        <p style="margin:4px 0;font-size:14px;font-weight:700;color:#F59E0B">{price}</p>
                        {(p.Beds.HasValue || p.AreaSqft.HasValue ? $"<p style=\"margin:4px 0;font-size:12px;color:#94a3b8\">{(p.Beds.HasValue ? p.Beds + " BR · " : "")}{(p.AreaSqft.HasValue ? p.AreaSqft.Value.ToString("N0") + " sqft" : "")}</p>" : "")}
                        <a href="{siteUrl}/Properties/{p.Slug}" style="display:inline-block;margin-top:6px;padding:6px 14px;background:#F59E0B;color:#fff;border-radius:6px;font-size:12px;font-weight:700;text-decoration:none">View listing</a>
                      </td>
                    </tr></table>
                  </td>
                </tr>
                """);
        }
        return sb.ToString();
    }

    private static string BuildDigestEmail(string? recipientName, int listingCount, string htmlListings, string siteUrl)
    {
        return $"""
        <div style="max-width:600px;margin:0 auto;font-family:Arial,sans-serif;color:#0f172a">
          <div style="background:linear-gradient(135deg,#F59E0B,#06B6D4);padding:28px 32px;border-radius:12px 12px 0 0">
            <h1 style="color:#fff;margin:0;font-size:24px">804 Avenue</h1>
            <p style="color:rgba(255,255,255,.85);margin:6px 0 0;font-size:14px">Abu Dhabi's integrated property &amp; contracting partner</p>
          </div>
          <div style="background:#fff;padding:28px 32px;border:1px solid #e2e8f0;border-top:none">
            <p style="font-size:16px">Hi {(string.IsNullOrWhiteSpace(recipientName) ? "there" : recipientName)},</p>
            <p style="color:#475569">Here are <strong>{listingCount} new listings</strong> added this week across Abu Dhabi and the UAE.</p>
            <table width="100%" style="border-collapse:collapse">
              {htmlListings}
            </table>
            <div style="text-align:center;margin:24px 0">
              <a href="{siteUrl}/Properties" style="display:inline-block;padding:14px 28px;background:#F59E0B;color:#fff;border-radius:8px;font-weight:700;text-decoration:none;font-size:15px">Browse All Properties</a>
            </div>
          </div>
          <div style="padding:16px 32px;background:#f8fafc;border:1px solid #e2e8f0;border-top:none;border-radius:0 0 12px 12px;font-size:12px;color:#94a3b8;text-align:center">
            You're receiving this as a 804 Avenue subscriber. <a href="{siteUrl}/Account/Dashboard" style="color:#F59E0B">Manage preferences</a>.
          </div>
        </div>
        """;
    }
}
