namespace Avenue804.Web.Domain;

public class LookupCategory
{
    public int Id { get; set; }

    /// <summary>Stable code, e.g. InquiryTopic (for code lookups).</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public ICollection<LookupValue> Values { get; set; } = new List<LookupValue>();
}
