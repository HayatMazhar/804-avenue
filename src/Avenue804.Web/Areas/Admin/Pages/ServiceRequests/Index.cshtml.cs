using System.Text;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.ServiceRequests;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;

    public int? Tab { get; set; }
    public int QuoteCount { get; set; }
    public int AmcCount { get; set; }
    public int TicketCount { get; set; }
    public List<ServiceQuoteRequest> QuoteRequests { get; set; } = [];
    public List<AmcRequest> AmcRequests { get; set; } = [];
    public List<MaintenanceTicket> Tickets { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int? tab, string? export, CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "service-requests";
        Tab = tab;

        // CSV Exports
        if (export == "quotes")
        {
            var all = await _db.ServiceQuoteRequests.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
            var csv = new StringBuilder("Date,Name,Email,Phone,Service,Area(sqm),Location,Estimate,Status\n");
            foreach (var r in all) csv.AppendLine($"{r.CreatedAt:yyyy-MM-dd},{r.Name},{r.Email},{r.Phone},{r.ServiceType},{r.AreaSqm},{r.Location},{r.EstimateRange},{r.Status}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "service-quotes.csv");
        }
        if (export == "amc")
        {
            var all = await _db.AmcRequests.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
            var csv = new StringBuilder("Date,Name,Email,Phone,BuildingType,Floors,Units,Area,Estimate,Status\n");
            foreach (var r in all) csv.AppendLine($"{r.CreatedAt:yyyy-MM-dd},{r.Name},{r.Email},{r.Phone},{r.BuildingType},{r.NumberOfFloors},{r.NumberOfUnits},{r.TotalAreaSqm},{r.EstimateRange},{r.Status}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "amc-requests.csv");
        }
        if (export == "tickets")
        {
            var all = await _db.MaintenanceTickets.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
            var csv = new StringBuilder("Date,Name,Email,Phone,Priority,Building,Issue,Status\n");
            foreach (var t in all) csv.AppendLine($"{t.CreatedAt:yyyy-MM-dd},{t.Name},{t.Email},{t.Phone},{t.Priority},{t.BuildingOrLocation},\"{t.IssueDescription}\",{t.Status}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "maintenance-tickets.csv");
        }

        QuoteCount = await _db.ServiceQuoteRequests.CountAsync(ct);
        AmcCount = await _db.AmcRequests.CountAsync(ct);
        TicketCount = await _db.MaintenanceTickets.CountAsync(ct);

        if (!tab.HasValue)
            QuoteRequests = await _db.ServiceQuoteRequests.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        else if (tab == 1)
            AmcRequests = await _db.AmcRequests.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        else
            Tickets = await _db.MaintenanceTickets
                .OrderBy(t => t.Priority == TicketPriority.Emergency ? 0 : t.Priority == TicketPriority.High ? 1 : 2)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync(ct);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, string type, ServiceRequestStatus? status, AmcRequestStatus? amcStatus, TicketStatus? ticketStatus, int? tab, CancellationToken ct = default)
    {
        if (type == "quote" && status.HasValue)
        {
            var r = await _db.ServiceQuoteRequests.FindAsync(id, ct);
            if (r != null) { r.Status = status.Value; await _db.SaveChangesAsync(ct); }
        }
        else if (type == "amc" && amcStatus.HasValue)
        {
            var r = await _db.AmcRequests.FindAsync(id, ct);
            if (r != null) { r.Status = amcStatus.Value; await _db.SaveChangesAsync(ct); }
        }
        else if (type == "ticket" && ticketStatus.HasValue)
        {
            var t = await _db.MaintenanceTickets.FindAsync(id, ct);
            if (t != null)
            {
                t.Status = ticketStatus.Value;
                if (ticketStatus == TicketStatus.Resolved) t.ResolvedAt = DateTimeOffset.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }

        TempData["ToastOk"] = "Status updated.";
        return RedirectToPage(new { tab = tab ?? (type == "amc" ? 1 : type == "ticket" ? 2 : (int?)null) });
    }
}
