using Avenue804.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using static Avenue804.Web.Areas.Admin.Pages.Agents.CreateModel;

namespace Avenue804.Web.Areas.Admin.Pages.Agents;

[Authorize(Policy = "AdminAccess")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public EditModel(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db; _users = users;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public AgentForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "agents";
        var a = await _db.Agents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (a == null) return NotFound();
        // Show email instead of raw userId if possible
        string? userDisplay = a.UserId;
        if (!string.IsNullOrWhiteSpace(a.UserId))
        {
            var u = await _users.FindByIdAsync(a.UserId);
            userDisplay = u?.Email ?? a.UserId;
        }
        Form = new()
        {
            Name = a.Name, Title = a.Title, Phone = a.Phone, Email = a.Email,
            WhatsAppNumber = a.WhatsAppNumber, AvatarUrl = a.AvatarUrl, Bio = a.Bio,
            IsActive = a.IsActive, ResponseTime = a.ResponseTime, ResponseRatePct = a.ResponseRatePct,
            UserId = userDisplay
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "agents";
        if (!ModelState.IsValid) return Page();
        var a = await _db.Agents.FirstOrDefaultAsync(x => x.Id == Id, ct);
        if (a == null) return NotFound();

        a.Name = Form.Name.Trim(); a.Title = Form.Title?.Trim(); a.Phone = Form.Phone?.Trim();
        a.Email = Form.Email?.Trim(); a.WhatsAppNumber = Form.WhatsAppNumber?.Trim();
        a.AvatarUrl = Form.AvatarUrl?.Trim(); a.Bio = Form.Bio?.Trim(); a.IsActive = Form.IsActive;
        a.ResponseTime = Form.ResponseTime?.Trim(); a.ResponseRatePct = Form.ResponseRatePct;

        // Resolve UserId by email lookup
        if (!string.IsNullOrWhiteSpace(Form.UserId))
        {
            var linked = Form.UserId.Contains('@')
                ? await _users.FindByEmailAsync(Form.UserId.Trim())
                : await _users.FindByIdAsync(Form.UserId.Trim());
            a.UserId = linked?.Id ?? Form.UserId.Trim();
        }
        else { a.UserId = null; }

        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Agent saved.";
        return RedirectToPage(new { id = Id });
    }
}
