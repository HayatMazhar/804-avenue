using Avenue804.Web.Configuration;
using Avenue804.Web.Models.Public;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class MaintenanceModel : PageModel
{
    private readonly IContentBlockService _content;

    public MaintenanceModel(IContentBlockService content) => _content = content;

    public ServicePageHeroModel Hero { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var map = await _content.GetPublishedBodiesAsync(
        [
            ContentBlockSlugs.PageMaintenanceHeroTitle, ContentBlockSlugs.PageMaintenanceHeroSubtitle,
            ContentBlockSlugs.MaintenanceSvc1Title, ContentBlockSlugs.MaintenanceSvc1Body,
            ContentBlockSlugs.MaintenanceSvc2Title, ContentBlockSlugs.MaintenanceSvc2Body,
            ContentBlockSlugs.MaintenanceSvc3Title, ContentBlockSlugs.MaintenanceSvc3Body,
            ContentBlockSlugs.MaintenanceSvc4Title, ContentBlockSlugs.MaintenanceSvc4Body,
            ContentBlockSlugs.MaintenanceSvc5Title, ContentBlockSlugs.MaintenanceSvc5Body,
            ContentBlockSlugs.MaintenanceWhyItems,
            ContentBlockSlugs.MaintenanceContractTitle, ContentBlockSlugs.MaintenanceContractBody,
            ContentBlockSlugs.MaintenanceContractFeatures,
            ContentBlockSlugs.MaintenanceCtaTitle, ContentBlockSlugs.MaintenanceCtaBody
        ], cancellationToken);

        Hero = ServicePageHeroModel.FromMap(map, ContentBlockSlugs.PageMaintenanceHeroTitle, ContentBlockSlugs.PageMaintenanceHeroSubtitle);
    }
}
