namespace Avenue804.Web.Domain;

public enum BuildingType
{
    Residential,
    Commercial,
    Retail,
    Industrial,
    Mixed
}

public enum AmcRequestStatus { New, InReview, QuoteSent, Accepted, Declined }

public class AmcRequest
{
    public int Id { get; set; }

    // Contact
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }

    // Building details
    public BuildingType BuildingType { get; set; }
    public int? NumberOfFloors { get; set; }
    public int? NumberOfUnits { get; set; }
    public decimal? TotalAreaSqm { get; set; }
    public string? Location { get; set; }

    // Services needed (stored as comma-separated codes)
    public string? ServicesNeeded { get; set; }

    // Calculator estimate
    public string? EstimateRange { get; set; }

    public AmcRequestStatus Status { get; set; } = AmcRequestStatus.New;
    public string? AdminNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
