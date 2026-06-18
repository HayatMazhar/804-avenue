using Avenue804.Web.Configuration;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Services;

/// <summary>
/// Per-request cache of CMS body lookups so a single page render only hits
/// the DB (or memory cache) once per distinct slug.
/// </summary>
public interface ICmsTextProvider
{
    /// <summary>Returns the published body for <paramref name="slug"/> or null.</summary>
    Task<string?> GetAsync(string slug, CancellationToken ct = default);

    /// <summary>Returns body or <paramref name="fallback"/>.</summary>
    Task<string> GetOrAsync(string slug, string fallback, CancellationToken ct = default);

    /// <summary>Preloads all globally registered slugs. Safe to call multiple times.</summary>
    Task PreloadAsync(CancellationToken ct = default);

    /// <summary>
    /// Synchronous lookup; returns admin-edited body if set, otherwise the fallback.
    /// Safe to call from Razor views after <see cref="PreloadAsync"/> has been awaited
    /// (the shared <c>_Layout</c> does this once per request). If preload hasn't run,
    /// performs a one-time sync preload to keep views simple.
    /// </summary>
    string this[string slug, string fallback] { get; }

    /// <summary>Same as the indexer, but returns an <see cref="IHtmlContent"/> so the value renders as raw HTML.</summary>
    IHtmlContent Html(string slug, string fallback);
}

public class CmsTextProvider : ICmsTextProvider
{
    private readonly IContentBlockService _blocks;
    private readonly Dictionary<string, string?> _cache = new(StringComparer.OrdinalIgnoreCase);
    private bool _preloaded;

    public CmsTextProvider(IContentBlockService blocks)
    {
        _blocks = blocks;
    }

    public async Task PreloadAsync(CancellationToken ct = default)
    {
        if (_preloaded) return;
        _preloaded = true;

        var slugs = CmsTextRegistry.AllSlugs;
        if (slugs.Count == 0) return;

        var bodies = await _blocks.GetPublishedBodiesAsync(slugs, ct);
        foreach (var slug in slugs)
        {
            var key = slug.Trim().ToLowerInvariant();
            _cache[key] = bodies.TryGetValue(key, out var v) ? v : null;
        }
    }

    public async Task<string?> GetAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var key = slug.Trim().ToLowerInvariant();

        if (!_preloaded)
        {
            await PreloadAsync(ct);
        }

        if (_cache.TryGetValue(key, out var cached)) return cached;

        // Slug not in registry: do a one-off lookup so even ad-hoc slugs work.
        var body = await _blocks.GetPublishedBodyAsync(key, ct);
        _cache[key] = body;
        return body;
    }

    public async Task<string> GetOrAsync(string slug, string fallback, CancellationToken ct = default)
    {
        var body = await GetAsync(slug, ct);
        return string.IsNullOrWhiteSpace(body) ? fallback : body;
    }

    public string this[string slug, string fallback]
    {
        get
        {
            if (string.IsNullOrWhiteSpace(slug)) return fallback;
            var key = slug.Trim().ToLowerInvariant();

            if (_cache.TryGetValue(key, out var cached))
                return string.IsNullOrWhiteSpace(cached) ? fallback : cached!;

            // Layout normally preloads via PreloadAsync(); this branch covers
            // pages that render before preload completes or use ad-hoc slugs.
            if (!_preloaded)
            {
                PreloadAsync().GetAwaiter().GetResult();
                if (_cache.TryGetValue(key, out var afterPreload))
                    return string.IsNullOrWhiteSpace(afterPreload) ? fallback : afterPreload!;
            }

            var body = _blocks.GetPublishedBodyAsync(key).GetAwaiter().GetResult();
            _cache[key] = body;
            return string.IsNullOrWhiteSpace(body) ? fallback : body!;
        }
    }

    public IHtmlContent Html(string slug, string fallback) => new HtmlString(this[slug, fallback]);
}
