using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Models.Forms;

public class PropertyListingInquiryFormModel
{
    [Required, StringLength(200)]
    [Display(Name = "Name")]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    [Display(Name = "Mobile number")]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, Display(Name = "I want to")]
    public int? WantToLookupValueId { get; set; }

    [Required, Display(Name = "Property type")]
    public int? PropertyTypeLookupValueId { get; set; }

    [Display(Name = "Property details")]
    public int? PropertyDetailLookupValueId { get; set; }

    [StringLength(8000)]
    [Display(Name = "Requirement details")]
    public string? RequirementDetails { get; set; }

    [StringLength(300)]
    [Display(Name = "Area")]
    public string? Area { get; set; }

    [Required, Display(Name = "Budget")]
    public int? BudgetLookupValueId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Expected move-in date")]
    public DateTime? ExpectedMoveInDate { get; set; }
}
