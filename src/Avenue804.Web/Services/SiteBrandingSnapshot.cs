namespace Avenue804.Web.Services;

public sealed class SiteBrandingSnapshot
{
    public required string CompanyDisplayName { get; init; }
    public required string NavTagline { get; init; }
    public required string PhoneE164 { get; init; }
    public required string PhoneDisplay { get; init; }
    public required string Email { get; init; }
    public required string Address { get; init; }
    public required string WhatsAppDigits { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? InstagramUrl { get; init; }
    public string? LinkedInUrl { get; init; }
    public string? FacebookUrl { get; init; }
    public required string FooterTagline { get; init; }
    public required string DefaultMetaDescription { get; init; }
    public required string DefaultListingCurrency { get; init; }
    public required string CopyrightOwner { get; init; }

    public string TelHref => PhoneE164.StartsWith('+') ? "tel:" + PhoneE164 : "tel:+" + PhoneE164.TrimStart('+');
    public string WhatsAppHref => "https://wa.me/" + WhatsAppDigits.TrimStart('+');
}
