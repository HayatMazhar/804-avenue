using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Forms;

namespace Avenue804.Web.Services;

public class PropertyListingInquirySubmitter : IPropertyListingInquirySubmitter
{
    private readonly ApplicationDbContext _db;
    private readonly ILookupService _lookups;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;

    public PropertyListingInquirySubmitter(ApplicationDbContext db, ILookupService lookups, IEmailSender email, IConfiguration cfg, IFeatureFlagService flags)
    {
        _db = db;
        _lookups = lookups;
        _email = email;
        _cfg = cfg;
        _flags = flags;
    }

    public async Task<bool> TrySubmitAsync(PropertyListingInquiryFormModel input, CancellationToken cancellationToken = default)
    {
        int? want = await ResolveRequiredAsync(input.WantToLookupValueId, LookupCategories.PropertyInquiryWantTo, cancellationToken);
        int? pType = await ResolveRequiredAsync(input.PropertyTypeLookupValueId, LookupCategories.PropertyInquiryPropertyType, cancellationToken);
        int? pDetail = await ResolveOptionalAsync(input.PropertyDetailLookupValueId, LookupCategories.PropertyInquiryPropertyDetail, cancellationToken);
        int? budget = await ResolveRequiredAsync(input.BudgetLookupValueId, LookupCategories.PropertyInquiryBudget, cancellationToken);

        if (input.WantToLookupValueId is int && want == null) return false;
        if (input.PropertyTypeLookupValueId is int && pType == null) return false;
        if (input.BudgetLookupValueId is int && budget == null) return false;
        if (input.PropertyDetailLookupValueId is int && pDetail == null) return false;

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

        if (await _flags.IsEnabledAsync(FeatureFlags.UserEmailConfirm, cancellationToken))
        {
            await _email.SendAsync(input.Email.Trim(),
                "We received your property inquiry — 804 Avenue",
                $"""
                <p>Dear {input.Name},</p>
                <p>Thank you for your inquiry. Our team will review your requirements and contact you shortly.</p>
                <p><strong>Your reference details:</strong></p>
                <ul>
                  <li>Area / preference: {input.Area ?? "—"}</li>
                  <li>Requirements: {input.RequirementDetails ?? "—"}</li>
                </ul>
                <p>If you have any questions, reply to this email or call us at +971 50 43 99 804.</p>
                <p>— The 804 Avenue Team</p>
                """,
                cancellationToken);
        }

        if (!await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, cancellationToken))
            return true;

        // Notify admin
        var adminEmail = _cfg["Site:Email"] ?? _cfg["Seed:AdminEmail"] ?? "info@804avenue.com";
        await _email.SendAsync(adminEmail,
            $"New Property Inquiry from {input.Name}",
            $"""
            <h3>New property inquiry received</h3>
            <table>
              <tr><td><strong>Name:</strong></td><td>{input.Name}</td></tr>
              <tr><td><strong>Email:</strong></td><td>{input.Email}</td></tr>
              <tr><td><strong>Phone:</strong></td><td>{input.Phone}</td></tr>
              <tr><td><strong>Area:</strong></td><td>{input.Area ?? "—"}</td></tr>
              <tr><td><strong>Requirements:</strong></td><td>{input.RequirementDetails ?? "—"}</td></tr>
            </table>
            <p><a href="/Admin/PropertyInquiries">View in admin</a></p>
            """,
            cancellationToken);

        return true;
    }

    private async Task<int?> ResolveOptionalAsync(int? id, string category, CancellationToken ct)
    {
        if (id is not int v) return null;
        return (await _lookups.FindValueAsync(v, category, ct)) is not null ? v : null;
    }

    private async Task<int?> ResolveRequiredAsync(int? id, string category, CancellationToken ct)
    {
        if (id is not int v) return null;
        return (await _lookups.FindValueAsync(v, category, ct)) is not null ? v : null;
    }
}
