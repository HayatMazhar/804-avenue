using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Models.Admin;

public class LookupCategoryForm
{
    [Required, StringLength(80)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}
