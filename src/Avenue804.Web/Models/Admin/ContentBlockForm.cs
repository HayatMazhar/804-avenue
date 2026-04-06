using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Models.Admin;

public class ContentBlockForm
{
    [Required, StringLength(160)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Title { get; set; }

    [Required, StringLength(20000)]
    public string Body { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
}
