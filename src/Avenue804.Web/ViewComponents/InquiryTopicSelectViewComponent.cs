using Avenue804.Web.Configuration;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Avenue804.Web.ViewComponents;

public class InquiryTopicSelectViewComponent : ViewComponent
{
    private readonly ILookupService _lookups;

    public InquiryTopicSelectViewComponent(ILookupService lookups) => _lookups = lookups;

    public async Task<IViewComponentResult> InvokeAsync(
        string inputName = "TopicLookupValueId",
        string? selectId = null,
        bool optional = true,
        string? selectedValue = null,
        string fieldWrapperClass = "modal-field",
        CancellationToken cancellationToken = default)
    {
        var items = await _lookups.GetActiveValuesAsync(LookupCategories.InquiryTopic, cancellationToken);
        ViewData["InputName"] = inputName;
        ViewData["SelectId"] = selectId ?? "inquiryTopic";
        ViewData["Optional"] = optional;
        ViewData["SelectedValue"] = selectedValue;
        ViewData["FieldWrapperClass"] = fieldWrapperClass;
        return View(items);
    }
}
