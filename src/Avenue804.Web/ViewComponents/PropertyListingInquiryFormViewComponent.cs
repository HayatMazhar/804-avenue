using Avenue804.Web.Configuration;
using Avenue804.Web.Models.Public;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avenue804.Web.ViewComponents;

public class PropertyListingInquiryFormViewComponent : ViewComponent
{
    private readonly ILookupService _lookups;

    public PropertyListingInquiryFormViewComponent(ILookupService lookups) => _lookups = lookups;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var vm = new PropertyListingInquiryFormViewModel
        {
            WantToOptions = await _lookups.GetActiveValuesAsync(LookupCategories.PropertyInquiryWantTo, cancellationToken),
            PropertyTypeOptions = await _lookups.GetActiveValuesAsync(LookupCategories.PropertyInquiryPropertyType, cancellationToken),
            PropertyDetailOptions = await _lookups.GetActiveValuesAsync(LookupCategories.PropertyInquiryPropertyDetail, cancellationToken),
            BudgetOptions = await _lookups.GetActiveValuesAsync(LookupCategories.PropertyInquiryBudget, cancellationToken)
        };
        return View(vm);
    }
}
