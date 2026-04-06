namespace Avenue804.Web.Configuration;

/// <summary>Published content block slugs used by Razor pages (admin can edit bodies).</summary>
public static class ContentBlockSlugs
{
    public const string HomeHeroEyebrow = "home.hero.eyebrow";
    public const string HomeHeroSubtitle = "home.hero.subtitle";
    public const string AboutStripLead = "about.strip.lead";

    public const string PageAboutHeroTitle = "page.about.hero.title";
    public const string PageAboutHeroSubtitle = "page.about.hero.subtitle";

    public const string PageContractingHeroTitle = "page.contracting.hero.title";
    public const string PageContractingHeroSubtitle = "page.contracting.hero.subtitle";

    public const string PageMaintenanceHeroTitle = "page.maintenance.hero.title";
    public const string PageMaintenanceHeroSubtitle = "page.maintenance.hero.subtitle";

    public const string PageFacilityHeroTitle = "page.facility.hero.title";
    public const string PageFacilityHeroSubtitle = "page.facility.hero.subtitle";

    public static IReadOnlyList<string> AllWiredSlugs { get; } =
    [
        HomeHeroEyebrow,
        HomeHeroSubtitle,
        AboutStripLead,
        PageAboutHeroTitle,
        PageAboutHeroSubtitle,
        PageContractingHeroTitle,
        PageContractingHeroSubtitle,
        PageMaintenanceHeroTitle,
        PageMaintenanceHeroSubtitle,
        PageFacilityHeroTitle,
        PageFacilityHeroSubtitle
    ];
}
