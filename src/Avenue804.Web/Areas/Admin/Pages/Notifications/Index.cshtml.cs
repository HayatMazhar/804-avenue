using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Notifications;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) => _db = db;
    public List<AdminNotification> Items { get; set; } = [];
    public int UnreadCount { get; set; }

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "notifications";
        Items = await _db.AdminNotifications.OrderByDescending(n => n.CreatedAt).Take(50).ToListAsync(ct);
        UnreadCount = Items.Count(n => !n.IsRead);
        ViewData["UnreadNotifs"] = UnreadCount;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        await _db.AdminNotifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);
        TempData["ToastOk"] = "All notifications marked as read.";
        return RedirectToPage();
    }
}
