namespace Avenue804.Web.Configuration;

/// <summary>
/// File-based defaults for public site branding. Database <see cref="Domain.SiteSetting"/> rows override these by key.
/// </summary>
public class SiteOptions
{
    public const string SectionName = "Site";

    public string CompanyDisplayName { get; set; } = "804 Avenue";

    /// <summary>Shown under logo (e.g. Properties &amp; Contracting).</summary>
    public string NavTagline { get; set; } = "Properties & Contracting";

    /// <summary>E.164 without spaces for tel: and wa.me links.</summary>
    public string PhoneE164 { get; set; } = "+971504399804";

    public string PhoneDisplay { get; set; } = "+971 50 43 99 804";

    public string Email { get; set; } = "info@804avenue.ae";

    public string Address { get; set; } =
        "Office #540, 5th Floor, Al Ghaith Tower, Abu Dhabi, UAE";

    /// <summary>Digits only for wa.me/971...</summary>
    public string WhatsAppDigits { get; set; } = "971504399804";

    public string? WebsiteUrl { get; set; } = "https://www.804avenue.ae";

    public string? InstagramUrl { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? FacebookUrl { get; set; }

    public string FooterTagline { get; set; } =
        "Delivering Excellence & Trust — Abu Dhabi's integrated partner for real estate, construction, and property maintenance.";

    public string DefaultMetaDescription { get; set; } =
        "804 Avenue Properties and Contracting — Abu Dhabi's trusted partner for real estate, construction, and maintenance across the UAE.";

    public string DefaultListingCurrency { get; set; } = "AED";

    public string CopyrightOwner { get; set; } = "804 Avenue Properties and Contracting";
}
