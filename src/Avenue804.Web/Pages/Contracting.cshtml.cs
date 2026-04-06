using Avenue804.Web.Configuration;
using Avenue804.Web.Models.Public;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages;

public class ContractingModel : PageModel
{
    private readonly IContentBlockService _content;

    public ContractingModel(IContentBlockService content) => _content = content;

    public ServicePageHeroModel Hero { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken cancellationToken = default)
    {
        var map = await _content.GetPublishedBodiesAsync(
            [ContentBlockSlugs.PageContractingHeroTitle, ContentBlockSlugs.PageContractingHeroSubtitle],
            cancellationToken);
        Hero = ServicePageHeroModel.FromMap(map, ContentBlockSlugs.PageContractingHeroTitle, ContentBlockSlugs.PageContractingHeroSubtitle);
    }
}
