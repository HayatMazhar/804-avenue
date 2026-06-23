namespace Avenue804.Web.Domain;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public BlogCategory Category { get; set; }

    public string? Excerpt { get; set; }
    /// <summary>HTML or markdown body — admin-trusted content (rendered with @Html.Raw, same pattern as AreaGuide.Overview).</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>Comma-separated keyword tags.</summary>
    public string? Tags { get; set; }

    public string? CoverImageUrl { get; set; }
    public string? AuthorName { get; set; }

    // SEO
    public string? SeoTitle { get; set; }
    public string? SeoMetaDescription { get; set; }
    public string? FocusKeyword { get; set; }

    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
