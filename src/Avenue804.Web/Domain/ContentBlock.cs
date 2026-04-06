namespace Avenue804.Web.Domain;

/// <summary>Editable HTML/text fragments for public pages (slug-addressable).</summary>
public class ContentBlock
{
    public int Id { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? Title { get; set; }

    /// <summary>HTML or plain text; only trusted admins should edit.</summary>
    public string Body { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
