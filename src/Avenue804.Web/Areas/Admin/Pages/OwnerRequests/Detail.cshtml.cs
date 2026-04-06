using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.OwnerRequests;

[Authorize(Policy = "AdminOnly")]
public class DetailModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public DetailModel(ApplicationDbContext db) => _db = db;

    public OwnerListingRequest? Row { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "owner";
        Row = await _db.OwnerListingRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (Row == null)
            return NotFound();
        return Page();
    }
}
