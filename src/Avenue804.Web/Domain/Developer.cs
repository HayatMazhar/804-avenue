namespace Avenue804.Web.Domain;

public class Developer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    // Company profile — "Company Overview" (brief introduction)
    public string? Description { get; set; }
    public string? Mission { get; set; }
    public string? Vision { get; set; }
    public string? Philosophy { get; set; }

    public string? Headquarters { get; set; }
    public int? EstablishedYear { get; set; }

    // Achievements & statistics
    public int? YearsInBusiness { get; set; }
    public int? TotalProjects { get; set; }
    public int? UnitsDelivered { get; set; }
    public string? AwardsRecognition { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PropertyListing> Listings { get; set; } = [];
}
