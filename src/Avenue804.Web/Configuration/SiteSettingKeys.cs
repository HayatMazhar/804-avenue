using System.Linq;
using System.Reflection;

namespace Avenue804.Web.Configuration;

/// <summary>Keys for <see cref="Domain.SiteSetting"/> and merge with <see cref="SiteOptions"/>.</summary>
public static class SiteSettingKeys
{
    public const string CompanyDisplayName = nameof(SiteOptions.CompanyDisplayName);
    public const string NavTagline = nameof(SiteOptions.NavTagline);
    public const string PhoneE164 = nameof(SiteOptions.PhoneE164);
    public const string PhoneDisplay = nameof(SiteOptions.PhoneDisplay);
    public const string Email = nameof(SiteOptions.Email);
    public const string Address = nameof(SiteOptions.Address);
    public const string WhatsAppDigits = nameof(SiteOptions.WhatsAppDigits);
    public const string WebsiteUrl = nameof(SiteOptions.WebsiteUrl);
    public const string InstagramUrl = nameof(SiteOptions.InstagramUrl);
    public const string LinkedInUrl = nameof(SiteOptions.LinkedInUrl);
    public const string FacebookUrl = nameof(SiteOptions.FacebookUrl);
    public const string TikTokUrl = nameof(SiteOptions.TikTokUrl);
    public const string YouTubeUrl = nameof(SiteOptions.YouTubeUrl);
    public const string FooterTagline = nameof(SiteOptions.FooterTagline);
    public const string DefaultMetaDescription = nameof(SiteOptions.DefaultMetaDescription);
    public const string DefaultListingCurrency = nameof(SiteOptions.DefaultListingCurrency);
    public const string CopyrightOwner = nameof(SiteOptions.CopyrightOwner);

    /// <summary>All defined keys (for admin pickers).</summary>
    public static IReadOnlyList<string> AllDefinedKeys { get; } = typeof(SiteSettingKeys)
        .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
        .Select(f => f.GetValue(null) as string)
        .Where(s => !string.IsNullOrEmpty(s))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
        .ToList();
}
