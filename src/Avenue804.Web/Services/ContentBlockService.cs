using Avenue804.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Services;

public class ContentBlockService : IContentBlockService
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public ContentBlockService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<string?> GetPublishedBodyAsync(string slug, CancellationToken cancellationToken = default)
    {
        var key = "content:" + slug.Trim().ToLowerInvariant();
        if (_cache.TryGetValue(key, out string? body))
            return body;

        var norm = slug.Trim().ToLowerInvariant();
        var block = await _db.ContentBlocks.AsNoTracking()
            .FirstOrDefaultAsync(
                b => b.Slug == norm && b.IsPublished,
                cancellationToken);

        var result = block?.Body;
        if (result is not null)
        {
            _cache.Set(key, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        }

        return result;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetPublishedBodiesAsync(
        IReadOnlyList<string> slugs,
        CancellationToken cancellationToken = default)
    {
        var normalized = slugs
            .Select(s => s.Trim().ToLowerInvariant())
            .Where(s => s.Length > 0)
            .Distinct()
            .ToList();

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var needDb = new List<string>();

        foreach (var slug in normalized)
        {
            var cacheKey = "content:" + slug;
            if (_cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
                result[slug] = cached;
            else
                needDb.Add(slug);
        }

        if (needDb.Count > 0)
        {
            var rows = await _db.ContentBlocks.AsNoTracking()
                .Where(b => b.IsPublished && needDb.Contains(b.Slug))
                .ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                var s = row.Slug.Trim().ToLowerInvariant();
                result[s] = row.Body;
                _cache.Set("content:" + s, row.Body, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
            }
        }

        return result;
    }

    public static void Invalidate(IMemoryCache cache, string slug) =>
        cache.Remove("content:" + slug.Trim().ToLowerInvariant());
}
