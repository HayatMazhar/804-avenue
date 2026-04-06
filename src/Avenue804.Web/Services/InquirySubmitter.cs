using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Forms;

namespace Avenue804.Web.Services;

public class InquirySubmitter : IInquirySubmitter
{
    private readonly ApplicationDbContext _db;
    private readonly ILookupService _lookups;

    public InquirySubmitter(ApplicationDbContext db, ILookupService lookups)
    {
        _db = db;
        _lookups = lookups;
    }

    public async Task SubmitAsync(InquiryFormModel input, CancellationToken cancellationToken = default)
    {
        int? topicId = null;
        if (input.TopicLookupValueId is int tid)
        {
            var v = await _lookups.FindValueAsync(tid, LookupCategories.InquiryTopic, cancellationToken);
            if (v is not null)
                topicId = tid;
        }

        _db.Inquiries.Add(new Inquiry
        {
            Name = input.Name.Trim(),
            Email = input.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(input.Phone) ? null : input.Phone.Trim(),
            Subject = input.Subject.Trim(),
            Message = input.Message.Trim(),
            TopicLookupValueId = topicId,
            Status = InquiryStatus.New
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
