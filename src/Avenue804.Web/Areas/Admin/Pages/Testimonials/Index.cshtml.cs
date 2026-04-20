using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Testimonials;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db) => _db = db;

    public List<Testimonial> Items { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        ViewData["AdminSection"] = "testimonials";
        Items = await _db.Testimonials.OrderBy(t => t.SortOrder).ThenByDescending(t => t.CreatedAt).ToListAsync(ct);
    }
}
