using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Agents;

[Authorize(Policy = "AdminAccess")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty] public AgentForm Form { get; set; } = new();

    public void OnGet() { ViewData["AdminSection"] = "agents"; }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "agents";
        if (!ModelState.IsValid) return Page();

        _db.Agents.Add(new Agent
        {
            Name = Form.Name.Trim(),
            Title = Form.Title?.Trim(),
            Phone = Form.Phone?.Trim(),
            Email = Form.Email?.Trim(),
            WhatsAppNumber = Form.WhatsAppNumber?.Trim(),
            AvatarUrl = Form.AvatarUrl?.Trim(),
            Bio = Form.Bio?.Trim(),
            IsActive = Form.IsActive
        });

        await _db.SaveChangesAsync(ct);
        TempData["ToastOk"] = "Agent created.";
        return RedirectToPage("./Index");
    }

    public class AgentForm
    {
        [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
        [StringLength(200)] public string? Title { get; set; }
        [StringLength(50)] public string? Phone { get; set; }
        [EmailAddress, StringLength(256)] public string? Email { get; set; }
        [StringLength(50)] public string? WhatsAppNumber { get; set; }
        [StringLength(2000), Url] public string? AvatarUrl { get; set; }
        [StringLength(1000)] public string? Bio { get; set; }
        [StringLength(100)] public string? ResponseTime { get; set; }
        public int? ResponseRatePct { get; set; }
        [StringLength(450)] public string? UserId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
