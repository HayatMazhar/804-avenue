using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminOnly")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public EditModel(ApplicationDbContext db) => _db = db;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public PropertyListingForm Form { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        var entity = await _db.PropertyListings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();

        Form = MapFrom(entity);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        var entity = await _db.PropertyListings.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity == null)
            return NotFound();

        if (!ModelState.IsValid)
            return Page();

        var baseSlug = string.IsNullOrWhiteSpace(Form.Slug)
            ? SlugGenerator.FromTitle(Form.Title)
            : SlugGenerator.FromTitle(Form.Slug.Trim(), Form.Title);
        var slug = await UniqueSlug.ForPropertyListingAsync(_db, baseSlug, Id, cancellationToken);

        entity.Title = Form.Title.Trim();
        entity.Slug = slug;
        entity.Price = Form.Price;
        entity.Currency = string.IsNullOrWhiteSpace(Form.Currency) ? "AED" : Form.Currency.Trim();
        entity.OfferType = Form.OfferType;
        entity.Location = string.IsNullOrWhiteSpace(Form.Location) ? null : Form.Location.Trim();
        entity.Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim();
        entity.Beds = Form.Beds;
        entity.Baths = Form.Baths;
        entity.AreaSqft = Form.AreaSqft;
        entity.MainImageUrl = string.IsNullOrWhiteSpace(Form.MainImageUrl) ? null : Form.MainImageUrl.Trim();
        entity.IsPublished = Form.IsPublished;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Listing saved.";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(CancellationToken cancellationToken = default)
    {
        var entity = await _db.PropertyListings.FirstOrDefaultAsync(x => x.Id == Id, cancellationToken);
        if (entity != null)
        {
            _db.PropertyListings.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        TempData["ToastOk"] = "Listing deleted.";
        return RedirectToPage("./Index");
    }

    private static PropertyListingForm MapFrom(PropertyListing e) => new()
    {
        Title = e.Title,
        Slug = e.Slug,
        Price = e.Price,
        Currency = e.Currency,
        OfferType = e.OfferType,
        Location = e.Location,
        Description = e.Description,
        Beds = e.Beds,
        Baths = e.Baths,
        AreaSqft = e.AreaSqft,
        MainImageUrl = e.MainImageUrl,
        IsPublished = e.IsPublished
    };
}
