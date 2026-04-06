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

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        ViewData["AdminSection"] = "dashboard";
        InquiryCount = await _db.Inquiries.CountAsync(cancellationToken);
        NewInquiryCount = await _db.Inquiries.CountAsync(i => i.Status == InquiryStatus.New, cancellationToken);
        PropertyListingInquiryCount = await _db.PropertyListingInquiries.CountAsync(cancellationToken);
        NewPropertyListingInquiryCount = await _db.PropertyListingInquiries.CountAsync(
            i => i.Status == InquiryStatus.New, cancellationToken);
        OwnerListingCount = await _db.OwnerListingRequests.CountAsync(cancellationToken);
        PublishedListings = await _db.PropertyListings.CountAsync(p => p.IsPublished, cancellationToken);
        PublishedProjects = await _db.PortfolioProjects.CountAsync(p => p.IsPublished, cancellationToken);
    }
}
