namespace Avenue804.Web.Domain;

public class NewsletterSubscriber
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Preferences { get; set; }  // comma-sep: "sale,rent,offplan"
    public bool IsActive { get; set; } = true;
    public DateTimeOffset SubscribedAt { get; set; } = DateTimeOffset.UtcNow;
}
