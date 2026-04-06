using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Domain;

public enum ListingOfferType
{
    [Display(Name = "For sale")]
    Sale = 0,
    [Display(Name = "For rent")]
    Rent = 1
}
