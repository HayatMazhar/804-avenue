using Avenue804.Web.Configuration;

namespace Avenue804.Web.Services;

/// <summary>
/// Central registry of all CMS slugs referenced from Razor pages. The
/// provider preloads all of these in a single DB hit per request.
/// </summary>
public static class CmsTextRegistry
{
    public static IReadOnlyList<string> AllSlugs { get; } =
        ContentBlockSlugs.AllWiredSlugs;
}
