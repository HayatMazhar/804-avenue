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
            [ContentBlockSlugs.PageMaintenanceHeroTitle, ContentBlockSlugs.PageMaintenanceHeroSubtitle],
            cancellationToken);
        Hero = ServicePageHeroModel.FromMap(map, ContentBlockSlugs.PageMaintenanceHeroTitle, ContentBlockSlugs.PageMaintenanceHeroSubtitle);
    }
}
