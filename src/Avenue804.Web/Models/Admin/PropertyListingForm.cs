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

    public bool IsPublished { get; set; }
}
