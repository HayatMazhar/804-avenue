using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Models.Admin;

public class SiteSettingForm
{
    [Required, StringLength(120)]
    public string Key { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Value { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
