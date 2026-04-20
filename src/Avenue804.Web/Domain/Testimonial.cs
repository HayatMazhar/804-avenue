namespace Avenue804.Web.Domain;

public class Testimonial
{
    public int Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public string Quote { get; set; } = string.Empty;
    public int Stars { get; set; } = 5;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
