using Avenue804.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private const string CacheKey = "feature-flags-v1";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public FeatureFlagService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> IsEnabledAsync(string flag, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.TryGetValue(flag, out var v) && v;
    }

    public async Task<IReadOnlyDictionary<string, bool>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out IReadOnlyDictionary<string, bool>? cached) && cached is not null)
            return cached;

        var rows = await _db.SiteSettings.AsNoTracking()
            .Where(s => s.Key.StartsWith("feature."))
            .ToListAsync(cancellationToken);

        var dict = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        // Seed defaults for any flag not yet in DB
        foreach (var (flag, _, _, defaultOn) in FeatureFlags.All)
            dict[flag] = defaultOn;

        // Override with DB values
        foreach (var row in rows)
        {
            var flagName = row.Key["feature.".Length..];
            dict[flagName] = string.Equals(row.Value.Trim(), "true", StringComparison.OrdinalIgnoreCase);
        }

        IReadOnlyDictionary<string, bool> result = dict;
        _cache.Set(CacheKey, result, CacheTtl);
        return result;
    }

    public void InvalidateCache() => _cache.Remove(CacheKey);
}
