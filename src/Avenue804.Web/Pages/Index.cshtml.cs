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
    private readonly IHomeStatsService _stats;

    public IndexModel(ApplicationDbContext db, IContentBlockService content, IHomeStatsService stats)
    {
        _db = db;
        _content = content;
        _stats = stats;
    }

    public IReadOnlyList<PropertyListing> FeaturedListings { get; private set; } = [];
    public IReadOnlyDictionary<string, string> Blocks { get; private set; } = new Dictionary<string, string>();
    public IReadOnlyList<Testimonial> Testimonials { get; private set; } = [];
    public HomeStats Stats { get; private set; } = new(0, 0, 0, 1);

    // Backward-compat properties still used by the partial
    public string? HeroEyebrowHtml => Blocks.TryGetValue(ContentBlockSlugs.HomeHeroEyebrow, out var v) ? v : null;
    public string? HeroSubtitleHtml => Blocks.TryGetValue(ContentBlockSlugs.HomeHeroSubtitle, out var v) ? v : null;
    public string? AboutStripLeadHtml => Blocks.TryGetValue(ContentBlockSlugs.AboutStripLead, out var v) ? v : null;

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        FeaturedListings = await _db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .Take(3)
            .ToListAsync(cancellationToken);

        Testimonials = await _db.Testimonials.AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder).ThenByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        Stats = await _stats.GetAsync(cancellationToken);

        Blocks = await _content.GetPublishedBodiesAsync(
        [
            ContentBlockSlugs.HomeHeroEyebrow, ContentBlockSlugs.HomeHeroSubtitle,
            ContentBlockSlugs.HomeHeroHeadline, ContentBlockSlugs.HomeHeroTitle,
            ContentBlockSlugs.HomeHeroPropertiesLink,
            ContentBlockSlugs.HomeServiceCardMaintLead,
            ContentBlockSlugs.HomeServiceCardAmcLead,
            ContentBlockSlugs.HomeServiceCardContrLead,
            ContentBlockSlugs.HomeServiceCardFmLead,
            ContentBlockSlugs.AboutStripLead,
            ContentBlockSlugs.HomeAboutTag, ContentBlockSlugs.HomeAboutTitle,
            ContentBlockSlugs.HomeAboutCards,
            ContentBlockSlugs.HomeStatsItems,
            ContentBlockSlugs.HomeReTag, ContentBlockSlugs.HomeReTitle,
            ContentBlockSlugs.HomeReCards,
            ContentBlockSlugs.HomeContractingTag, ContentBlockSlugs.HomeContractingTitle,
            ContentBlockSlugs.HomeContractingLead, ContentBlockSlugs.HomeContractingCards,
            ContentBlockSlugs.HomeMaintenanceTag, ContentBlockSlugs.HomeMaintenanceTitle,
            ContentBlockSlugs.HomeMaintenanceLead, ContentBlockSlugs.HomeMaintenanceCards,
            ContentBlockSlugs.HomeWhyTag, ContentBlockSlugs.HomeWhyTitle,
            ContentBlockSlugs.HomeWhyItems,
            ContentBlockSlugs.HomeTestiTag, ContentBlockSlugs.HomeTestiTitle,
            ContentBlockSlugs.HomeTestiItems,
            ContentBlockSlugs.HomeCtaTitle
        ], cancellationToken);
    }
}
