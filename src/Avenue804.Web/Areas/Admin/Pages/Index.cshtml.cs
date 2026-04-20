using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public int InquiryCount { get; set; }
    public int NewInquiryCount { get; set; }
    public int OwnerListingCount { get; set; }
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

    // Last 7 days daily inquiry chart data (JSON)
    public string InquiryChartJson { get; set; } = "[]";
    public string ViewChartJson { get; set; } = "[]";

    // Top 5 most-viewed listings
    public List<PropertyListing> TopListings { get; set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["AdminSection"] = "dashboard";

        var now = DateTimeOffset.UtcNow;

        InquiryCount = await _db.Inquiries.CountAsync(cancellationToken);
        NewInquiryCount = await _db.Inquiries.CountAsync(i => i.Status == InquiryStatus.New, cancellationToken);
        PropertyListingInquiryCount = await _db.PropertyListingInquiries.CountAsync(cancellationToken);
        NewPropertyListingInquiryCount = await _db.PropertyListingInquiries.CountAsync(i => i.Status == InquiryStatus.New, cancellationToken);
        OwnerListingCount = await _db.OwnerListingRequests.CountAsync(cancellationToken);
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
        ViewData["UnreadNotifs"] = UnreadNotifications;

        // Last 7 days inquiries per day
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

        // Top listings by views
        TopListings = await _db.PropertyListings
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.ViewCount)
            .Take(5)
            .ToListAsync(cancellationToken);
    }
}
