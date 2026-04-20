using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Models.Forms;

namespace Avenue804.Web.Services;

public class InquirySubmitter : IInquirySubmitter
{
    private readonly ApplicationDbContext _db;
    private readonly ILookupService _lookups;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;

    public InquirySubmitter(ApplicationDbContext db, ILookupService lookups, IEmailSender email, IConfiguration cfg, IFeatureFlagService flags)
    {
        _db = db;
        _lookups = lookups;
        _email = email;
        _cfg = cfg;
        _flags = flags;
    }

    public async Task SubmitAsync(InquiryFormModel input, CancellationToken cancellationToken = default)
    {
        int? topicId = null;
        if (input.TopicLookupValueId is int tid)
        {
            var v = await _lookups.FindValueAsync(tid, LookupCategories.InquiryTopic, cancellationToken);
            if (v is not null) topicId = tid;
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

        if (!await _flags.IsEnabledAsync(FeatureFlags.EmailNotifications, cancellationToken))
            return;

        // Notify admin
        var adminEmail = _cfg["Site:Email"] ?? _cfg["Seed:AdminEmail"] ?? "info@804avenue.com";
        await _email.SendAsync(adminEmail,
            $"New Contact Inquiry — {input.Subject}",
            $"""
            <h3>New contact inquiry received</h3>
            <table>
              <tr><td><strong>Name:</strong></td><td>{input.Name}</td></tr>
              <tr><td><strong>Email:</strong></td><td>{input.Email}</td></tr>
              <tr><td><strong>Phone:</strong></td><td>{input.Phone ?? "—"}</td></tr>
              <tr><td><strong>Subject:</strong></td><td>{input.Subject}</td></tr>
              <tr><td><strong>Message:</strong></td><td>{input.Message}</td></tr>
            </table>
            <p><a href="/Admin/Inquiries">View in admin</a></p>
            """,
            cancellationToken);
    }
}
