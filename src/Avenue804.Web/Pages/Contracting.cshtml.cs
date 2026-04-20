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
        [
            ContentBlockSlugs.PageContractingHeroTitle, ContentBlockSlugs.PageContractingHeroSubtitle,
            ContentBlockSlugs.ContractingSvc1Title, ContentBlockSlugs.ContractingSvc1Body,
            ContentBlockSlugs.ContractingSvc2Title, ContentBlockSlugs.ContractingSvc2Body,
            ContentBlockSlugs.ContractingSvc3Title, ContentBlockSlugs.ContractingSvc3Body,
            ContentBlockSlugs.ContractingSvc4Title, ContentBlockSlugs.ContractingSvc4Body,
            ContentBlockSlugs.ContractingSvc5Title, ContentBlockSlugs.ContractingSvc5Body,
            ContentBlockSlugs.ContractingSvc6Title, ContentBlockSlugs.ContractingSvc6Body,
            ContentBlockSlugs.ContractingSvc7Title, ContentBlockSlugs.ContractingSvc7Body,
            ContentBlockSlugs.ContractingProcessTitle, ContentBlockSlugs.ContractingProcessLead,
            ContentBlockSlugs.ContractingProcessSteps,
            ContentBlockSlugs.ContractingMaterialsItems,
            ContentBlockSlugs.ContractingCtaTitle, ContentBlockSlugs.ContractingCtaBody
        ], cancellationToken);

        Hero = ServicePageHeroModel.FromMap(map, ContentBlockSlugs.PageContractingHeroTitle, ContentBlockSlugs.PageContractingHeroSubtitle);
    }
}
