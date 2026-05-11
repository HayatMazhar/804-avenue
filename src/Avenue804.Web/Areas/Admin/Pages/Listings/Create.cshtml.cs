using System.Text.Json;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Areas.Admin.Pages.Listings;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public CreateModel(ApplicationDbContext db) => _db = db;

    [BindProperty]
    public PropertyListingForm Form { get; set; } = new();

    public void OnGet()
    {
        ViewData["AdminSection"] = "listings";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken = default)
    {
        ViewData["AdminSection"] = "listings";
        if (!ModelState.IsValid)
            return Page();

        var baseSlug = string.IsNullOrWhiteSpace(Form.Slug)
            ? SlugGenerator.FromTitle(Form.Title)
            : SlugGenerator.FromTitle(Form.Slug.Trim(), Form.Title);
        var slug = await UniqueSlug.ForPropertyListingAsync(_db, baseSlug, null, cancellationToken);

        var entity = new PropertyListing
        {
            Title = Form.Title.Trim(),
            Slug = slug,
            Price = Form.Price,
            Currency = string.IsNullOrWhiteSpace(Form.Currency) ? "AED" : Form.Currency.Trim(),
            OfferType = Form.OfferType,
            Location = string.IsNullOrWhiteSpace(Form.Location) ? null : Form.Location.Trim(),
            PropertyCategory = string.IsNullOrWhiteSpace(Form.PropertyCategory) ? null : Form.PropertyCategory.Trim(),
            PropertyType = string.IsNullOrWhiteSpace(Form.PropertyType) ? null : Form.PropertyType.Trim(),
            Emirates = string.IsNullOrWhiteSpace(Form.Emirates) ? null : Form.Emirates.Trim(),
            Description = string.IsNullOrWhiteSpace(Form.Description) ? null : Form.Description.Trim(),
            Beds = Form.Beds,
            Baths = Form.Baths,
            AreaSqft = Form.AreaSqft,
            MainImageUrl = string.IsNullOrWhiteSpace(Form.MainImageUrl) ? null : Form.MainImageUrl.Trim(),
            GalleryImagesJson = ParseGallery(Form.GalleryImagesJson),
            IsPublished = Form.IsPublished,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.PropertyListings.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        TempData["ToastOk"] = "Listing created.";
        return RedirectToPage("./Edit", new { id = entity.Id });
    }

    private static string? ParseGallery(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        var urls = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                      .Where(u => u.StartsWith("http", StringComparison.OrdinalIgnoreCase)).ToArray();
        return urls.Length == 0 ? null : JsonSerializer.Serialize(urls);
    }
}
