using System.Reflection;
using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Avenue804.Web.Services;

public class SiteBrandingService : ISiteBrandingService
{
    public const string CacheKey = "site-branding-snapshot-v1";

    private readonly ApplicationDbContext _db;
    private readonly SiteOptions _defaults;
    private readonly IMemoryCache _cache;

    public SiteBrandingService(
        ApplicationDbContext db,
        IOptions<SiteOptions> defaults,
        IMemoryCache cache)
    {
        _db = db;
        _defaults = defaults.Value;
        _cache = cache;
    }

    public void InvalidateCache() => _cache.Remove(CacheKey);

    public async Task<SiteBrandingSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out SiteBrandingSnapshot? cached) && cached is not null)
            return cached;

        var fromDb = await _db.SiteSettings.AsNoTracking().ToListAsync(cancellationToken);
        var map = fromDb.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

        string Pick(string key, string fallback) =>
            map.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : fallback;

        var snap = new SiteBrandingSnapshot
        {
            CompanyDisplayName = Pick(SiteSettingKeys.CompanyDisplayName, _defaults.CompanyDisplayName),
            NavTagline = Pick(SiteSettingKeys.NavTagline, _defaults.NavTagline),
            PhoneE164 = Pick(SiteSettingKeys.PhoneE164, _defaults.PhoneE164),
            PhoneDisplay = Pick(SiteSettingKeys.PhoneDisplay, _defaults.PhoneDisplay),
            Email = Pick(SiteSettingKeys.Email, _defaults.Email),
            Address = Pick(SiteSettingKeys.Address, _defaults.Address),
            WhatsAppDigits = Pick(SiteSettingKeys.WhatsAppDigits, _defaults.WhatsAppDigits),
            WebsiteUrl = NullIfEmpty(Pick(SiteSettingKeys.WebsiteUrl, _defaults.WebsiteUrl ?? "")),
            InstagramUrl = NullIfEmpty(Pick(SiteSettingKeys.InstagramUrl, _defaults.InstagramUrl ?? "")),
            LinkedInUrl = NullIfEmpty(Pick(SiteSettingKeys.LinkedInUrl, _defaults.LinkedInUrl ?? "")),
            FacebookUrl = NullIfEmpty(Pick(SiteSettingKeys.FacebookUrl, _defaults.FacebookUrl ?? "")),
            TikTokUrl = NullIfEmpty(Pick(SiteSettingKeys.TikTokUrl, _defaults.TikTokUrl ?? "")),
            YouTubeUrl = NullIfEmpty(Pick(SiteSettingKeys.YouTubeUrl, _defaults.YouTubeUrl ?? "")),
            FooterTagline = Pick(SiteSettingKeys.FooterTagline, _defaults.FooterTagline),
            DefaultMetaDescription = Pick(SiteSettingKeys.DefaultMetaDescription, _defaults.DefaultMetaDescription),
            DefaultListingCurrency = Pick(SiteSettingKeys.DefaultListingCurrency, _defaults.DefaultListingCurrency),
            CopyrightOwner = Pick(SiteSettingKeys.CopyrightOwner, _defaults.CopyrightOwner)
        };

        _cache.Set(CacheKey, snap, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
        });

        return snap;
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s;

    /// <summary>Seed missing rows from current <see cref="SiteOptions"/> (reflection by key name).</summary>
    public static IReadOnlyList<Domain.SiteSetting> BuildDefaultRowsFromOptions(SiteOptions o)
    {
        var list = new List<Domain.SiteSetting>();
        foreach (var prop in typeof(SiteOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.PropertyType != typeof(string))
                continue;
            var val = prop.GetValue(o) as string;
            if (string.IsNullOrWhiteSpace(val))
                continue;
            list.Add(new Domain.SiteSetting
            {
                Key = prop.Name,
                Value = val.Trim(),
                Description = "Seeded from appsettings Site section"
            });
        }

        return list;
    }
}
