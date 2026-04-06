using Avenue804.Web.Models.Forms;

namespace Avenue804.Web.Services;

public interface IPropertyListingInquirySubmitter
{
    /// <returns><see langword="false"/> if lookup ids were invalid or missing required selections.</returns>
    Task<bool> TrySubmitAsync(PropertyListingInquiryFormModel input, CancellationToken cancellationToken = default);
}
