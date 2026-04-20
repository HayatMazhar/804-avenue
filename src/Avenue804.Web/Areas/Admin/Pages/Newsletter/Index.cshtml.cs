using System.Text;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Newsletter;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public List<NewsletterSubscriber> Items { get; set; } = [];
    public int TotalCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string? export, CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "newsletter";
        Items = await _db.NewsletterSubscribers.OrderByDescending(s => s.SubscribedAt).ToListAsync(ct);
        TotalCount = Items.Count;

        if (export == "csv")
        {
            var csv = new StringBuilder("Email,Name,Preferences,Subscribed\n");
            foreach (var s in Items)
                csv.AppendLine($"{s.Email},{s.Name},{s.Preferences},{s.SubscribedAt:yyyy-MM-dd}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "newsletter-subscribers.csv");
        }

        return Page();
    }
}
