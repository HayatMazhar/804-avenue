using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminAccess")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    // ── KPIs ──────────────────────────────────────────────
    public int InquiryCount { get; set; }
    public int NewInquiryCount { get; set; }
    public int OwnerListingCount { get; set; }
    public int NewOwnerListingCount { get; set; }
    public int PublishedListings { get; set; }
    public int PublishedProjects { get; set; }
    public int PropertyListingInquiryCount { get; set; }
    public int NewPropertyListingInquiryCount { get; set; }
    public int PublicUserCount { get; set; }
    public int SavedPropertyCount { get; set; }
    public int PendingRatingCount { get; set; }
    public int TotalViews { get; set; }
    public int NewServiceQuotes { get; set; }
    public int NewAmcRequests { get; set; }
    public int OpenEmergencyTickets { get; set; }
    public int UnreadNotifications { get; set; }
    public int TestimonialCount { get; set; }
    public int NewsletterSubscribers { get; set; }

    public string InquiryChartJson { get; set; } = "[]";

    public List<PropertyListing> TopListings { get; set; } = [];
    public List<ActivityItem> RecentActivity { get; set; } = [];

    public record ActivityItem(string Kind, string Title, string Meta, DateTimeOffset At, string? Url);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["AdminSection"] = "dashboard";
        ViewData["Title"] = "Dashboard";

        var now = DateTimeOffset.UtcNow;

        InquiryCount = await _db.Inquiries.CountAsync(cancellationToken);
        NewInquiryCount = await _db.Inquiries.CountAsync(i => i.Status == InquiryStatus.New, cancellationToken);
        PropertyListingInquiryCount = await _db.PropertyListingInquiries.CountAsync(cancellationToken);
        NewPropertyListingInquiryCount = await _db.PropertyListingInquiries.CountAsync(i => i.Status == InquiryStatus.New, cancellationToken);
        OwnerListingCount = await _db.OwnerListingRequests.CountAsync(cancellationToken);
        NewOwnerListingCount = await _db.OwnerListingRequests
            .CountAsync(r => r.CreatedAt >= now.AddDays(-7), cancellationToken);
        PublishedListings = await _db.PropertyListings.CountAsync(p => p.IsPublished, cancellationToken);
        PublishedProjects = await _db.PortfolioProjects.CountAsync(p => p.IsPublished, cancellationToken);
        PublicUserCount = await _db.Users.CountAsync(u => u.IsPublicUser, cancellationToken);
        SavedPropertyCount = await _db.SavedProperties.CountAsync(cancellationToken);
        PendingRatingCount = await _db.PropertyRatings.CountAsync(r => !r.IsApproved, cancellationToken);
        TotalViews = await _db.PropertyListings.SumAsync(p => (int?)p.ViewCount ?? 0, cancellationToken);
        NewServiceQuotes = await _db.ServiceQuoteRequests.CountAsync(r => r.Status == ServiceRequestStatus.New, cancellationToken);
        NewAmcRequests = await _db.AmcRequests.CountAsync(r => r.Status == AmcRequestStatus.New, cancellationToken);
        OpenEmergencyTickets = await _db.MaintenanceTickets.CountAsync(t => t.Priority == TicketPriority.Emergency && t.Status == TicketStatus.New, cancellationToken);
        UnreadNotifications = await _db.AdminNotifications.CountAsync(n => !n.IsRead, cancellationToken);
        TestimonialCount = await _db.Testimonials.CountAsync(cancellationToken);
        NewsletterSubscribers = await _db.NewsletterSubscribers.CountAsync(s => s.IsActive, cancellationToken);
        ViewData["UnreadNotifs"] = UnreadNotifications;

        var sevenDaysAgo = now.AddDays(-6).Date;
        var inquiryByDay = await _db.PropertyListingInquiries
            .Where(i => i.CreatedAt >= sevenDaysAgo)
            .GroupBy(i => i.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var labels = new List<string>();
        var inquiryCounts = new List<int>();
        for (int d = 6; d >= 0; d--)
        {
            var date = now.AddDays(-d).Date;
            labels.Add(date.ToString("MMM d"));
            inquiryCounts.Add(inquiryByDay.FirstOrDefault(x => x.Date == date)?.Count ?? 0);
        }
        InquiryChartJson = System.Text.Json.JsonSerializer.Serialize(new { labels, data = inquiryCounts });

        TopListings = await _db.PropertyListings
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentPropInq = await _db.PropertyListingInquiries
            .OrderByDescending(i => i.CreatedAt)
            .Take(4)
            .Select(i => new { i.Id, i.Name, i.CreatedAt })
            .ToListAsync(cancellationToken);

        var recentInq = await _db.Inquiries
            .OrderByDescending(i => i.CreatedAt)
            .Take(3)
            .Select(i => new { i.Id, i.Name, i.Subject, i.CreatedAt })
            .ToListAsync(cancellationToken);

        var recentOwner = await _db.OwnerListingRequests
            .OrderByDescending(r => r.CreatedAt)
            .Take(3)
            .Select(r => new { r.Id, r.Name, r.CreatedAt })
            .ToListAsync(cancellationToken);

        var recentSvc = await _db.ServiceQuoteRequests
            .OrderByDescending(r => r.CreatedAt)
            .Take(3)
            .Select(r => new { r.Id, r.Name, r.CreatedAt })
            .ToListAsync(cancellationToken);

        var recentAmc = await _db.AmcRequests
            .OrderByDescending(r => r.CreatedAt)
            .Take(3)
            .Select(r => new { r.Id, r.Name, r.CreatedAt })
            .ToListAsync(cancellationToken);

        var feed = new List<ActivityItem>();
        feed.AddRange(recentPropInq.Select(i => new ActivityItem(
            "property", $"{i.Name} inquired about a property", "Property inquiry", i.CreatedAt,
            $"/Admin/PropertyInquiries/Detail?id={i.Id}")));
        feed.AddRange(recentInq.Select(i => new ActivityItem(
            "contact", $"{i.Name} sent a contact message", string.IsNullOrWhiteSpace(i.Subject) ? "Contact" : i.Subject, i.CreatedAt,
            $"/Admin/Inquiries/Detail?id={i.Id}")));
        feed.AddRange(recentOwner.Select(r => new ActivityItem(
            "owner", $"{r.Name} submitted a listing request", "Listing request", r.CreatedAt,
            $"/Admin/OwnerRequests/Detail?id={r.Id}")));
        feed.AddRange(recentSvc.Select(r => new ActivityItem(
            "service", $"{r.Name} requested a service quote", "Contracting", r.CreatedAt,
            "/Admin/ServiceRequests/Index")));
        feed.AddRange(recentAmc.Select(r => new ActivityItem(
            "amc", $"{r.Name} requested an AMC quote", "AMC", r.CreatedAt,
            "/Admin/ServiceRequests/Index")));

        RecentActivity = feed
            .OrderByDescending(a => a.At)
            .Take(8)
            .ToList();
    }
}
