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

    public bool IsPublished { get; set; }
}
