namespace Avenue804.Web.Domain;

public class Developer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Description { get; set; }
    public string? Headquarters { get; set; }
    public int? EstablishedYear { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PropertyListing> Listings { get; set; } = [];
}
