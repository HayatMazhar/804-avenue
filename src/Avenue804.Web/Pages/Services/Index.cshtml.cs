using Avenue804.Web.Configuration;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Services;

/// <summary>
/// Services hub — single jumping-off page that points to Maintenance, AMC,
/// Contracting and Facility Management. Replaces the previous behaviour where
/// the only "services" entry-point was the all-in-one OurServices page.
///
/// Copy is CMS-overridable via <see cref="ContentBlockSlugs.PageServicesHubTitle"/>,
/// <see cref="ContentBlockSlugs.PageServicesHubSubtitle"/> and
/// <see cref="ContentBlockSlugs.PageServicesHubLead"/>.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IContentBlockService _content;

    public IndexModel(IContentBlockService content)
    {
        _content = content;
    }

    public IReadOnlyDictionary<string, string> Blocks { get; private set; } = new Dictionary<string, string>();

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        Blocks = await _content.GetPublishedBodiesAsync(
        [
            ContentBlockSlugs.PageServicesHubEyebrow,
            ContentBlockSlugs.PageServicesHubTitle,
            ContentBlockSlugs.PageServicesHubSubtitle,
            ContentBlockSlugs.PageServicesHubLead,
        ], cancellationToken);
    }

    public string Block(string slug, string fallback) =>
        Blocks.TryGetValue(slug, out var v) && !string.IsNullOrWhiteSpace(v) ? v : fallback;
}
