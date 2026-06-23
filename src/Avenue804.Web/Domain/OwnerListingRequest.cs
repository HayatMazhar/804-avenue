namespace Avenue804.Web.Domain;

public class OwnerListingRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public OwnerListingIntent Intent { get; set; }
    public string? LocationOrTitle { get; set; }
    public string? Details { get; set; }

    /// <summary>Who the submitter is (Free Lancer / Company / Individual Owner / Developer).</summary>
    public string? IAmRole { get; set; }

    /// <summary>Broad property category selected on the form (Residential / Commercial).</summary>
    public string? PropertyCategory { get; set; }

    /// <summary>Specific sub-type (Apartment / Villa / Showroom / etc.).</summary>
    public string? PropertySubType { get; set; }

    /// <summary>Emirates selected on the form.</summary>
    public string? Emirate { get; set; }

    /// <summary>Community / area free-text entered on the form.</summary>
    public string? Area { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
