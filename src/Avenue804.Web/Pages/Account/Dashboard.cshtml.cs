using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.Account;

public class DashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DashboardModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public string? DisplayName { get; private set; }
    public string? Email { get; private set; }
    public string? AvatarUrl { get; private set; }
    public List<SavedProperty> SavedProperties { get; private set; } = [];
    public List<PropertyRating> Ratings { get; private set; } = [];
    public List<PropertyListingInquiry> Inquiries { get; private set; } = [];
    public List<SavedSearch> SavedSearches { get; private set; } = [];
    public List<ServiceQuoteRequest> ServiceQuotes { get; private set; } = [];
    public List<AmcRequest> AmcReqs { get; private set; } = [];
    public List<MaintenanceTicket> MaintTickets { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        DisplayName = user.DisplayName;
        Email = user.Email;
        AvatarUrl = user.AvatarUrl;

        SavedProperties = await _db.SavedProperties
            .Include(s => s.Listing)
            .Where(s => s.UserId == user.Id && s.Listing.IsPublished)
            .OrderByDescending(s => s.SavedAt)
            .ToListAsync(ct);

        Ratings = await _db.PropertyRatings
            .Include(r => r.Listing)
            .Where(r => r.UserId == user.Id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        SavedSearches = await _db.SavedSearches
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(ct);

        ServiceQuotes = await _db.ServiceQuoteRequests
            .Where(r => r.Email == user.Email).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        AmcReqs = await _db.AmcRequests
            .Where(r => r.Email == user.Email).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        MaintTickets = await _db.MaintenanceTickets
            .Where(r => r.Email == user.Email).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

        Inquiries = await _db.PropertyListingInquiries
            .Include(i => i.WantToLookup)
            .Include(i => i.PropertyTypeLookup)
            .Include(i => i.BudgetLookup)
            .Where(i => i.Email == user.Email)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(ct);

        return Page();
    }
}
