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
    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
