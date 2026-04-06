using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Forms;

namespace Avenue804.Web.Services;

public class PropertyListingInquirySubmitter : IPropertyListingInquirySubmitter
{
    private readonly ApplicationDbContext _db;
    private readonly ILookupService _lookups;

    public PropertyListingInquirySubmitter(ApplicationDbContext db, ILookupService lookups)
    {
        _db = db;
        _lookups = lookups;
    }

    public async Task<bool> TrySubmitAsync(PropertyListingInquiryFormModel input, CancellationToken cancellationToken = default)
    {
        int? want = await ResolveRequiredAsync(input.WantToLookupValueId, LookupCategories.PropertyInquiryWantTo, cancellationToken);
        int? pType = await ResolveRequiredAsync(input.PropertyTypeLookupValueId, LookupCategories.PropertyInquiryPropertyType, cancellationToken);
        int? pDetail = await ResolveOptionalAsync(input.PropertyDetailLookupValueId, LookupCategories.PropertyInquiryPropertyDetail, cancellationToken);
        int? budget = await ResolveRequiredAsync(input.BudgetLookupValueId, LookupCategories.PropertyInquiryBudget, cancellationToken);

        if (input.WantToLookupValueId is int wv && want == null)
            return false;
        if (input.PropertyTypeLookupValueId is int pt && pType == null)
            return false;
        if (input.BudgetLookupValueId is int b && budget == null)
            return false;
        if (input.PropertyDetailLookupValueId is int pd && pDetail == null)
            return false;

        DateOnly? moveIn = null;
        if (input.ExpectedMoveInDate is DateTime dt)
            moveIn = DateOnly.FromDateTime(dt.Date);

        _db.PropertyListingInquiries.Add(new PropertyListingInquiry
        {
            Name = input.Name.Trim(),
            Phone = input.Phone.Trim(),
            Email = input.Email.Trim(),
            WantToLookupValueId = want,
            PropertyTypeLookupValueId = pType,
            PropertyDetailLookupValueId = pDetail,
            RequirementDetails = string.IsNullOrWhiteSpace(input.RequirementDetails) ? null : input.RequirementDetails.Trim(),
            BudgetLookupValueId = budget,
            Area = string.IsNullOrWhiteSpace(input.Area) ? null : input.Area.Trim(),
            ExpectedMoveInDate = moveIn,
            Status = InquiryStatus.New
        });

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int?> ResolveOptionalAsync(int? id, string category, CancellationToken ct)
    {
        if (id is not int v)
            return null;
        return (await _lookups.FindValueAsync(v, category, ct)) is not null ? v : null;
    }

    private async Task<int?> ResolveRequiredAsync(int? id, string category, CancellationToken ct)
    {
        if (id is not int v)
            return null;
        return (await _lookups.FindValueAsync(v, category, ct)) is not null ? v : null;
    }
}
