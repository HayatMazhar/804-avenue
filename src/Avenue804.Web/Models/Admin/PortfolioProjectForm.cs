using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Models.Admin;

public class PortfolioProjectForm
{
    [Required]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [StringLength(320)]
    public string? Slug { get; set; }

    [StringLength(2000)]
    public string? Summary { get; set; }

    [StringLength(32000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? CoverImageUrl { get; set; }

    public bool IsPublished { get; set; }
}
