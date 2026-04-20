namespace Avenue804.Web.Models.Public;

/// <summary>Carries CMS content blocks for a service/about page. Supports arbitrary slug lookup.</summary>
public class ServicePageHeroModel
{
    private readonly IReadOnlyDictionary<string, string> _blocks;
    private readonly string _titleSlug;
    private readonly string _subtitleSlug;

    private ServicePageHeroModel(IReadOnlyDictionary<string, string> blocks, string titleSlug, string subtitleSlug)
    {
        _blocks = blocks;
        _titleSlug = titleSlug.ToLowerInvariant();
        _subtitleSlug = subtitleSlug.ToLowerInvariant();
    }

    /// <summary>CMS hero title HTML, or null when not published.</summary>
    public string? HeroTitleHtml => Get(_titleSlug);

    /// <summary>CMS hero subtitle HTML, or null when not published.</summary>
    public string? HeroSubtitleHtml => Get(_subtitleSlug);

    /// <summary>Returns the published body HTML for a given slug, or null.</summary>
    public string? Get(string slug) =>
        _blocks.TryGetValue(slug.ToLowerInvariant(), out var v) ? v : null;

    /// <summary>Returns the published body HTML for a given slug, or <paramref name="fallback"/>.</summary>
    public string GetOr(string slug, string fallback) => Get(slug) ?? fallback;

    public static ServicePageHeroModel FromMap(
        IReadOnlyDictionary<string, string> map,
        string titleSlug,
        string subtitleSlug) => new(map, titleSlug, subtitleSlug);
}
