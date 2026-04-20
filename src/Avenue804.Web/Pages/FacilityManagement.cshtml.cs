using Avenue804.Web.Configuration;
using Avenue804.Web.Models.Public;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class FacilityManagementModel : PageModel
{
    private readonly IContentBlockService _content;

    public FacilityManagementModel(IContentBlockService content) => _content = content;

    public ServicePageHeroModel Hero { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var map = await _content.GetPublishedBodiesAsync(
        [
            ContentBlockSlugs.PageFacilityHeroTitle, ContentBlockSlugs.PageFacilityHeroSubtitle,
            ContentBlockSlugs.FacilitySvc1Title, ContentBlockSlugs.FacilitySvc1Body,
            ContentBlockSlugs.FacilitySvc2Title, ContentBlockSlugs.FacilitySvc2Body,
            ContentBlockSlugs.FacilityWhyTag, ContentBlockSlugs.FacilityWhyTitle,
            ContentBlockSlugs.FacilityWhyItems,
            ContentBlockSlugs.FacilityCtaTitle, ContentBlockSlugs.FacilityCtaBody
        ], cancellationToken);

        Hero = ServicePageHeroModel.FromMap(map, ContentBlockSlugs.PageFacilityHeroTitle, ContentBlockSlugs.PageFacilityHeroSubtitle);
    }
}
