using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IContentBlockService _content;

    public IndexModel(ApplicationDbContext db, IContentBlockService content)
    {
        _db = db;
        _content = content;
    }

    public IReadOnlyList<PropertyListing> FeaturedListings { get; private set; } = [];

    public string? HeroEyebrowHtml { get; private set; }
    public string? HeroSubtitleHtml { get; private set; }
    public string? AboutStripLeadHtml { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        FeaturedListings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        HeroEyebrowHtml = await _content.GetPublishedBodyAsync(ContentBlockSlugs.HomeHeroEyebrow, cancellationToken);
        HeroSubtitleHtml = await _content.GetPublishedBodyAsync(ContentBlockSlugs.HomeHeroSubtitle, cancellationToken);
        AboutStripLeadHtml = await _content.GetPublishedBodyAsync(ContentBlockSlugs.AboutStripLead, cancellationToken);
    }
}
