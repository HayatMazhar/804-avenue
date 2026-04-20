using Avenue804.Web.Data;

namespace Avenue804.Web.Domain;

public class SavedSearch
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string? Label { get; set; }

    // Filter params (mirrors Properties/Index params)
    public string? Offer { get; set; }
    public string? Location { get; set; }
    public string? PropertyType { get; set; }
    public string? Budget { get; set; }
    public string? Keyword { get; set; }

    public bool AlertEnabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastAlertedAt { get; set; }
}
