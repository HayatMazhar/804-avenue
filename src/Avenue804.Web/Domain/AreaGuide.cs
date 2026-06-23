namespace Avenue804.Web.Domain;

public class AreaGuide
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;    // e.g. "Al Reem Island"
    public string Slug { get; set; } = string.Empty;    // e.g. "al-reem-island"
    public string? Emirate { get; set; }                // e.g. "Abu Dhabi"
    public string? City { get; set; }                   // e.g. "Abu Dhabi" (within emirate)
    public string? HeroImageUrl { get; set; }

    /// <summary>1–2 sentence summary shown on the index card and as the hero lede.</summary>
    public string? ShortDescription { get; set; }
    /// <summary>Long-form body copy. HTML allowed (rendered with @@Html.Raw).</summary>
    public string? Overview { get; set; }

    // Property stats (manually entered by admin)
    public decimal? AvgPriceSaleSqft { get; set; }      // AED per sqft
    public decimal? AvgRentYearly { get; set; }         // 1BR annual
    public string? PopularWith { get; set; }            // "Families, Expats"
    public string? NearbyLandmarks { get; set; }        // "ADGM, Al Reem Mall, Sorbonne"
    public string? SchoolsNearby { get; set; }
    public string? TransportLinks { get; set; }

    // ── Lifestyle & amenities (one entry per line — rendered with white-space: pre-line) ──
    public string? Amenities { get; set; }
    public string? Attractions { get; set; }
    public string? LifestyleServices { get; set; }
    public string? Shopping { get; set; }
    public string? Education { get; set; }
    public string? Dining { get; set; }
    public string? Healthcare { get; set; }
    public string? Transportation { get; set; }

    // ── Investment information ─────────────────────────
    public decimal? AverageRoiPercent { get; set; }     // 0-100, e.g. 6.50
    public decimal? RentalYieldPercent { get; set; }    // 0-100, e.g. 5.80
    public string? InvestmentInsights { get; set; }     // long-form market commentary

    public bool IsPublished { get; set; } = true;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
