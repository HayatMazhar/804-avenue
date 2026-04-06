namespace Avenue804.Web.Domain;

/// <summary>Key/value site configuration; overrides <see cref="Configuration.SiteOptions"/> when present.</summary>
public class SiteSetting
{
    public int Id { get; set; }

    /// <summary>Stable key, e.g. PhoneDisplay, Email (case-insensitive match).</summary>
    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
