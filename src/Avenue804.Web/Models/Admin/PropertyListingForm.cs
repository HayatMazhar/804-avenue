using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Domain;

namespace Avenue804.Web.Models.Admin;

public class PropertyListingForm
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(320)]
    public string? Slug { get; set; }

    public decimal? Price { get; set; }

    [StringLength(8)]
    public string Currency { get; set; } = "AED";

    public ListingOfferType OfferType { get; set; }

    [StringLength(400)]
    public string? Location { get; set; }

    /// <summary>Residential or Commercial</summary>
    [StringLength(20)]
    public string? PropertyCategory { get; set; }

    /// <summary>Subtype e.g. Apartment, Villa, Shell &amp; Core Offices</summary>
    [StringLength(80)]
    public string? PropertyType { get; set; }

    /// <summary>Emirate e.g. Abu Dhabi</summary>
    [StringLength(80)]
    public string? Emirates { get; set; }

    [StringLength(16000)]
    public string? Description { get; set; }

    [Range(0, 100)]
    public int? Beds { get; set; }

    [Range(0, 100)]
    public int? Baths { get; set; }

    [Range(0, 1_000_000)]
    public int? AreaSqft { get; set; }

    [StringLength(2000)]
    public string? MainImageUrl { get; set; }

    /// <summary>Admin enters one URL per line; stored as JSON array on entity.</summary>
    public string? GalleryImagesJson { get; set; }

    [StringLength(50)]
    public string? Label { get; set; }

    public bool IsVerified { get; set; }
    public ListingApprovalStatus ApprovalStatus { get; set; } = ListingApprovalStatus.Draft;
    public string? RejectionReason { get; set; }
    public ListingAvailabilityStatus AvailabilityStatus { get; set; } = ListingAvailabilityStatus.Available;
    public OccupancyStatus? OccupancyStatus { get; set; }

    // SEO overrides
    [StringLength(300)] public string? SeoTitle { get; set; }
    [StringLength(500)] public string? SeoDescription { get; set; }

    // Phase 2
    [StringLength(2000)] public string? FloorPlanUrl { get; set; }
    [StringLength(2000)] public string? VirtualTourUrl { get; set; }
    public bool IsOffPlan { get; set; }
    [StringLength(50)] public string? HandoverDate { get; set; }
    [StringLength(300)] public string? PaymentPlan { get; set; }
    [Range(0, 100)] public int? CompletionPercent { get; set; }
    public int? DeveloperId { get; set; }
    public string? AmenitiesJson { get; set; }  // comma-sep codes from checkboxes

    // ── Off-plan project introduction ──────────────────
    [StringLength(300)] public string? Subtitle { get; set; }
    [StringLength(50)] public string? ProjectStatus { get; set; }
    [StringLength(500)] public string? ProjectAddress { get; set; }

    // ── Off-plan property specifications ───────────────
    [StringLength(1000)] public string? UnitTypes { get; set; }
    [StringLength(100)] public string? BedroomOptions { get; set; }
    [StringLength(100)] public string? BathroomOptions { get; set; }
    [Range(0, 1_000_000)] public int? StartingSizeSqft { get; set; }
    [Range(0, 1000)] public int? TotalFloors { get; set; }
    [Range(0, 10000)] public int? TotalBuildings { get; set; }
    [Range(0, 1_000_000)] public int? TotalUnits { get; set; }

    // ── Off-plan structured payment plan ───────────────
    [Range(0, 100)] public int? DownPaymentPercent { get; set; }
    [Range(0, 100)] public int? DuringConstructionPercent { get; set; }
    [Range(0, 100)] public int? OnHandoverPercent { get; set; }

    // ── Off-plan features & amenities ──────────────────
    [StringLength(2000)] public string? KeyFeatures { get; set; }
    [StringLength(8000)] public string? AmenitiesDescription { get; set; }
    [StringLength(4000)] public string? NearbyLandmarks { get; set; }

    public bool IsPublished { get; set; }
}
