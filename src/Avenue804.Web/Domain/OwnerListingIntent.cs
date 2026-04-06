using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Domain;

public enum OwnerListingIntent
{
    [Display(Name = "For sale")]
    Sale = 0,
    [Display(Name = "For rent")]
    Rent = 1,
    [Display(Name = "Sale & rent")]
    Both = 2
}
