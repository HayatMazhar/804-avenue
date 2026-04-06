using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Models.Admin;

public class LookupValueForm
{
    [Required, StringLength(80)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(2000)]
    public string? Metadata { get; set; }
}
