using Avenue804.Web.Data;

namespace Avenue804.Web.Domain;

public class PropertyRating
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int ListingId { get; set; }
    public PropertyListing Listing { get; set; } = null!;

    /// <summary>1–5 stars.</summary>
    public int Stars { get; set; }

    public string? Review { get; set; }
    public bool IsApproved { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
