namespace Avenue804.Web.Domain;

public class LookupValue
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public LookupCategory? Category { get; set; }

    /// <summary>Optional machine code within category.</summary>
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Optional JSON for extra attributes.</summary>
    public string? Metadata { get; set; }
}
