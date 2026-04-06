using Avenue804.Web.Models.Forms;

namespace Avenue804.Web.Services;

public interface IInquirySubmitter
{
    Task SubmitAsync(InquiryFormModel input, CancellationToken cancellationToken = default);
}
