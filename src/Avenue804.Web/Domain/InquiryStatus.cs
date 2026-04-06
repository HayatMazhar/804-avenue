using System.ComponentModel.DataAnnotations;

namespace Avenue804.Web.Domain;

public enum InquiryStatus
{
    [Display(Name = "New")]
    New = 0,
    [Display(Name = "In progress")]
    InProgress = 1,
    [Display(Name = "Closed")]
    Closed = 2
}
