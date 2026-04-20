using Avenue804.Web.Configuration;
using Avenue804.Web.Models.Public;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class AboutModel : PageModel
{
    private readonly IContentBlockService _content;

    public AboutModel(IContentBlockService content) => _content = content;

    public ServicePageHeroModel Hero { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var map = await _content.GetPublishedBodiesAsync(
        [
            ContentBlockSlugs.PageAboutHeroTitle, ContentBlockSlugs.PageAboutHeroSubtitle,
            ContentBlockSlugs.AboutStoryTag, ContentBlockSlugs.AboutStoryTitle,
            ContentBlockSlugs.AboutStoryBody,
            ContentBlockSlugs.AboutVmTag, ContentBlockSlugs.AboutVisionBody,
            ContentBlockSlugs.AboutMissionBody,
            ContentBlockSlugs.AboutValuesTag, ContentBlockSlugs.AboutValuesItems,
            ContentBlockSlugs.AboutExpertiseTag, ContentBlockSlugs.AboutExpertiseLead,
            ContentBlockSlugs.AboutExpertiseItems,
            ContentBlockSlugs.AboutTeamLead, ContentBlockSlugs.AboutTeamItems,
            ContentBlockSlugs.AboutWcuTag, ContentBlockSlugs.AboutWcuItems
        ], cancellationToken);

        Hero = ServicePageHeroModel.FromMap(map, ContentBlockSlugs.PageAboutHeroTitle, ContentBlockSlugs.PageAboutHeroSubtitle);
    }
}
