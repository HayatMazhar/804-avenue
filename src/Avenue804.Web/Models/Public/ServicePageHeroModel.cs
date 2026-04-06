namespace Avenue804.Web.Models.Public;

/// <summary>Optional HTML for service/about page heroes (from <see cref="Domain.ContentBlock"/>).</summary>
public class ServicePageHeroModel
{
    public string? HeroTitleHtml { get; init; }
    public string? HeroSubtitleHtml { get; init; }

    public static ServicePageHeroModel FromMap(
        IReadOnlyDictionary<string, string> map,
        string titleSlug,
        string subtitleSlug)
    {
        var tk = titleSlug.Trim().ToLowerInvariant();
        var sk = subtitleSlug.Trim().ToLowerInvariant();
        map.TryGetValue(tk, out var title);
        map.TryGetValue(sk, out var sub);
        return new ServicePageHeroModel
        {
            HeroTitleHtml = title,
            HeroSubtitleHtml = sub
        };
    }
}
