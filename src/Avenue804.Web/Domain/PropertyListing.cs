namespace Avenue804.Web.Domain;

public class PropertyListing
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public decimal? Price { get; set; }
    public string Currency { get; set; } = "AED";
    public ListingOfferType OfferType { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public int? Beds { get; set; }
    public int? Baths { get; set; }
    public int? AreaSqft { get; set; }
    public string? MainImageUrl { get; set; }

    /// <summary>JSON array of additional image URLs: ["https://…","https://…"]</summary>
    public string? GalleryImagesJson { get; set; }

    public int? AgentId { get; set; }
    public Agent? Agent { get; set; }

    public int ViewCount { get; set; }
    public string? Label { get; set; }

    /// <summary>Residential or Commercial</summary>
    public string? PropertyCategory { get; set; }

    /// <summary>Subtype e.g. Apartment, Villa, Shell &amp; Core Offices, Showroom…</summary>
    public string? PropertyType { get; set; }

    /// <summary>Emirate e.g. Abu Dhabi</summary>
    public string? Emirates { get; set; }

    // ── Phase 2 additions ────────────────────────────────
    /// <summary>JSON array of amenity codes e.g. ["pool","gym","parking"]</summary>
    public string? AmenitiesJson { get; set; }

    public string? FloorPlanUrl { get; set; }
    public string? VirtualTourUrl { get; set; }

    // Off-plan / new projects
    public bool IsOffPlan { get; set; }
    public string? HandoverDate { get; set; }        // e.g. "Q4 2026"
    public string? PaymentPlan { get; set; }         // e.g. "40/60 — 40% on booking" (free-text fallback)
    public int? CompletionPercent { get; set; }      // 0-100

    // ── Off-plan project introduction ──────────────────
    /// <summary>Marketing subtitle/tagline shown under the project name on the off-plan detail page.</summary>
    public string? Subtitle { get; set; }
    /// <summary>Upcoming / Launching / Under Construction / Ready</summary>
    public string? ProjectStatus { get; set; }
    /// <summary>Full project address (separate from the searchable Location field).</summary>
    public string? ProjectAddress { get; set; }

    // ── Off-plan property specifications ───────────────
    /// <summary>Comma-separated unit types e.g. "1BR Apartments, 2BR Apartments, 3BR Villas"</summary>
    public string? UnitTypes { get; set; }
    /// <summary>Bedroom range e.g. "1-5"</summary>
    public string? BedroomOptions { get; set; }
    /// <summary>Bathroom range e.g. "1-6"</summary>
    public string? BathroomOptions { get; set; }
    public int? StartingSizeSqft { get; set; }
    public int? TotalFloors { get; set; }
    public int? TotalBuildings { get; set; }
    public int? TotalUnits { get; set; }

    // ── Off-plan payment plan (structured) ─────────────
    public int? DownPaymentPercent { get; set; }
    public int? DuringConstructionPercent { get; set; }
    public int? OnHandoverPercent { get; set; }

    // ── Off-plan features & amenities ──────────────────
    /// <summary>Comma-separated short tags / key feature chips.</summary>
    public string? KeyFeatures { get; set; }
    /// <summary>Long-form amenities description (rendered as paragraph copy).</summary>
    public string? AmenitiesDescription { get; set; }
    /// <summary>One nearby landmark per line e.g. "Dubai Mall – 10 Min"</summary>
    public string? NearbyLandmarks { get; set; }

    // Developer
    public int? DeveloperId { get; set; }
    public Developer? Developer { get; set; }

    // Price tracking
    public decimal? PreviousPrice { get; set; }
    /// <summary>JSON array: [{"date":"2025-01-01","price":1500000},…]</summary>
    public string? PriceHistoryJson { get; set; }

    // Verified badge (like Bayut TruCheck)
    public bool IsVerified { get; set; }

    // Approval workflow
    public ListingApprovalStatus ApprovalStatus { get; set; } = ListingApprovalStatus.Draft;
    public string? RejectionReason { get; set; }

    /// <summary>Market availability — shown as a public status badge (Under Offer / Rented / Sold).</summary>
    public ListingAvailabilityStatus AvailabilityStatus { get; set; } = ListingAvailabilityStatus.Available;

    /// <summary>Current occupancy of the property (Vacant / Occupied / Owner Occupied).</summary>
    public OccupancyStatus? OccupancyStatus { get; set; }

    // SEO overrides
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }

    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
