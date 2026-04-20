namespace Avenue804.Web.Domain;

public class AreaGuide
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;    // e.g. "Al Reem Island"
    public string Slug { get; set; } = string.Empty;    // e.g. "al-reem-island"
    public string? Emirate { get; set; }                // e.g. "Abu Dhabi"
    public string? HeroImageUrl { get; set; }
    public string? Overview { get; set; }               // Rich text / HTML

    // Property stats (manually entered by admin)
    public decimal? AvgPriceSaleSqft { get; set; }      // AED per sqft
    public decimal? AvgRentYearly { get; set; }         // 1BR annual
    public string? PopularWith { get; set; }            // "Families, Expats"
    public string? NearbyLandmarks { get; set; }        // "ADGM, Al Reem Mall, Sorbonne"
    public string? SchoolsNearby { get; set; }
    public string? TransportLinks { get; set; }

    public bool IsPublished { get; set; } = true;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
