namespace Avenue804.Web.Domain;

public class PortfolioProject
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
