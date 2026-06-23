namespace Avenue804.Web.Domain;

public enum BlogCategory
{
    RealEstateNews = 0,
    Investment = 1,
    CommunityGuide = 2,
    CompanyAnnouncements = 3,
    Contracting = 4,
    IndustryInsights = 5,
}

public static class BlogCategoryExtensions
{
    /// <summary>
    /// Human-readable label for a <see cref="BlogCategory"/>, e.g. <c>RealEstateNews → "Real Estate News"</c>.
    /// </summary>
    public static string ToDisplayString(this BlogCategory category) => category switch
    {
        BlogCategory.RealEstateNews => "Real Estate News",
        BlogCategory.Investment => "Investment",
        BlogCategory.CommunityGuide => "Community Guide",
        BlogCategory.CompanyAnnouncements => "Company Announcements",
        BlogCategory.Contracting => "Contracting & Maintenance",
        BlogCategory.IndustryInsights => "Industry Insights",
        _ => category.ToString()
    };

    /// <summary>All categories in display order.</summary>
    public static readonly BlogCategory[] All =
    [
        BlogCategory.RealEstateNews,
        BlogCategory.Investment,
        BlogCategory.CommunityGuide,
        BlogCategory.CompanyAnnouncements,
        BlogCategory.Contracting,
        BlogCategory.IndustryInsights,
    ];
}
