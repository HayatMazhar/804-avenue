namespace Avenue804.Web.Domain;

public class Agent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Phone { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Link agent to a portal user account.</summary>
    public string? UserId { get; set; }

    // Response rate fields
    public string? ResponseTime { get; set; }    // e.g. "Within 2 hours"
    public int? ResponseRatePct { get; set; }    // 0-100
    public DateTimeOffset? LastActiveAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PropertyListing> Listings { get; set; } = [];
}
