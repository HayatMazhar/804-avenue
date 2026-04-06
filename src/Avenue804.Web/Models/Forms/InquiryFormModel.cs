using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Configuration;

namespace Avenue804.Web.Models.Forms;

public class InquiryFormModel
{
    [Required, StringLength(200)]
    [Display(Name = "Full name")]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    /// <summary>Optional; subject line preset from <see cref="LookupCategories.InquiryTopic"/>.</summary>
    [Display(Name = "Topic")]
    public int? TopicLookupValueId { get; set; }

    [Required, StringLength(300)]
    [Display(Name = "Subject")]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(8000)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;
}
