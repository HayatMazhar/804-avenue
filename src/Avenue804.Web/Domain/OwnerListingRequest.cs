namespace Avenue804.Web.Domain;

public class OwnerListingRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public OwnerListingIntent Intent { get; set; }
    public string? LocationOrTitle { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
