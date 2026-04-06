using System.ComponentModel.DataAnnotations;
using Avenue804.Web.Domain;

namespace Avenue804.Web.Models.Forms;

public class OwnerListingFormModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public OwnerListingIntent Intent { get; set; }

    [StringLength(500)]
    public string? LocationOrTitle { get; set; }

    [StringLength(8000)]
    public string? Details { get; set; }
}
