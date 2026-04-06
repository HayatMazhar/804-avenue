using Avenue804.Web.Domain;

namespace Avenue804.Web.Models.Public;

public class PropertyListingInquiryFormViewModel
{
    public IReadOnlyList<LookupValue> WantToOptions { get; init; } = [];
    public IReadOnlyList<LookupValue> PropertyTypeOptions { get; init; } = [];
    public IReadOnlyList<LookupValue> PropertyDetailOptions { get; init; } = [];
    public IReadOnlyList<LookupValue> BudgetOptions { get; init; } = [];
}
