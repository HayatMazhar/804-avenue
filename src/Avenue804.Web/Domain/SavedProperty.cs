using Avenue804.Web.Data;

namespace Avenue804.Web.Domain;

public class SavedProperty
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int ListingId { get; set; }
    public PropertyListing Listing { get; set; } = null!;
    public DateTimeOffset SavedAt { get; set; } = DateTimeOffset.UtcNow;
}
