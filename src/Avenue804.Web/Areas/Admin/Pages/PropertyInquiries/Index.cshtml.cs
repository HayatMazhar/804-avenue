using System.Text;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.PropertyInquiries;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public const int PageSize = 25;
    public int PageNumber { get; set; } = 1;
    public int TotalCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public IList<PropertyListingInquiry> Items { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int p = 1, string? export = null, CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "property-inquiries";
        PageNumber = p < 1 ? 1 : p;

        if (export == "csv")
        {
            var all = await _db.PropertyListingInquiries.AsNoTracking()
                .Include(x => x.WantToLookup).Include(x => x.BudgetLookup)
                .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
            var csv = new StringBuilder();
            csv.AppendLine("Date,Name,Email,Phone,WantTo,PropertyType,Area,Budget,MoveIn,Status");
            foreach (var i in all)
                csv.AppendLine($"{i.CreatedAt:yyyy-MM-dd},\"{i.Name}\",{i.Email},{i.Phone},\"{i.WantToLookup?.DisplayName}\",\"{i.Area}\",\"{i.BudgetLookup?.DisplayName}\",{i.ExpectedMoveInDate},{i.Status}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"property-inquiries-{DateTimeOffset.UtcNow:yyyy-MM-dd}.csv");
        }

        TotalCount = await _db.PropertyListingInquiries.CountAsync(ct);
        Items = await _db.PropertyListingInquiries.AsNoTracking()
            .Include(x => x.WantToLookup).Include(x => x.BudgetLookup)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((PageNumber - 1) * PageSize).Take(PageSize).ToListAsync(ct);
        return Page();
    }
}
