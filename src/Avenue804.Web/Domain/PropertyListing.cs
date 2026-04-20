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

    // ── Phase 2 additions ────────────────────────────────
    /// <summary>JSON array of amenity codes e.g. ["pool","gym","parking"]</summary>
    public string? AmenitiesJson { get; set; }

    public string? FloorPlanUrl { get; set; }
    public string? VirtualTourUrl { get; set; }

    // Off-plan / new projects
    public bool IsOffPlan { get; set; }
    public string? HandoverDate { get; set; }        // e.g. "Q4 2026"
    public string? PaymentPlan { get; set; }         // e.g. "40/60 — 40% on booking"
    public int? CompletionPercent { get; set; }      // 0-100

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

    // SEO overrides
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }

    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
