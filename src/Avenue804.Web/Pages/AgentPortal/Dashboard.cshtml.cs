using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages.AgentPortal;

[Authorize(Policy = "PublicUser")]
public class AgentDashboardModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public AgentDashboardModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public Agent? Agent { get; private set; }
    public List<PropertyListing> MyListings { get; private set; } = [];
    public List<PropertyListingInquiry> Leads { get; private set; } = [];
    public Dictionary<int, int> SavesPerListing { get; private set; } = [];
    public int TotalViews { get; private set; }
    public int TotalSaves { get; private set; }
    public int OpenLeads { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Account/Login");

        // Find agent linked to this user
        Agent = await _db.Agents.AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsActive, ct);

        if (Agent == null)
        {
            TempData["ToastError"] = "No agent profile is linked to your account. Contact admin.";
            return RedirectToPage("/Account/Dashboard");
        }

        MyListings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.AgentId == Agent.Id)
            .OrderByDescending(p => p.ViewCount)
            .ToListAsync(ct);

        TotalViews = MyListings.Sum(p => p.ViewCount);

        // Saves per listing
        var listingIds = MyListings.Select(p => p.Id).ToList();
        SavesPerListing = await _db.SavedProperties
            .Where(s => listingIds.Contains(s.ListingId))
            .GroupBy(s => s.ListingId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
        TotalSaves = SavesPerListing.Values.Sum();

        // Leads — inquiries where the user explicitly emailed the agent, or for listings assigned to this agent
        var agentEmail = Agent.Email ?? "";
        Leads = await _db.PropertyListingInquiries
            .Include(i => i.WantToLookup).Include(i => i.BudgetLookup)
            .Where(i => listingIds.Count > 0)   // for all their listings
            .OrderByDescending(i => i.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        OpenLeads = Leads.Count(l => l.Status == InquiryStatus.New);

        return Page();
    }

    public async Task<IActionResult> OnPostUpdateLeadAsync(int leadId, InquiryStatus status, CancellationToken ct = default)
    {
        var lead = await _db.PropertyListingInquiries.FindAsync(leadId, ct);
        if (lead != null) { lead.Status = status; await _db.SaveChangesAsync(ct); }
        TempData["ToastOk"] = "Lead status updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveNoteAsync(int leadId, string? note, CancellationToken ct = default)
    {
        var lead = await _db.PropertyListingInquiries.FindAsync(leadId, ct);
        if (lead != null) { lead.AgentNote = note?.Trim(); await _db.SaveChangesAsync(ct); }
        TempData["ToastOk"] = "Note saved.";
        return RedirectToPage();
    }
}
