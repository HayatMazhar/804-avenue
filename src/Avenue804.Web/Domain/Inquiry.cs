namespace Avenue804.Web.Domain;

public class Inquiry
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public int? TopicLookupValueId { get; set; }
    public LookupValue? TopicLookup { get; set; }

    public InquiryStatus Status { get; set; } = InquiryStatus.New;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
