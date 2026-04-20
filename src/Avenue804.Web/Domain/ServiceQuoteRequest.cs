namespace Avenue804.Web.Domain;

public enum ServiceType
{
    CivilContracting,
    GypsumCeiling,
    GlassAluminum,
    PaintingFinishing,
    Flooring,
    FitOutRenovation,
    Signage,
    MaintenanceGeneral,
    MaintenanceMep,
    MaintenanceCivil,
    MaintenancePreventive,
    FacilityManagement,
    BundlePackage
}

public enum ServiceRequestStatus
{
    New,
    InReview,
    QuoteSent,
    Accepted,
    Declined
}

public class ServiceQuoteRequest
{
    public int Id { get; set; }

    // Contact
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Company { get; set; }

    // Service details
    public ServiceType ServiceType { get; set; }
    public string? ProjectDescription { get; set; }
    public decimal? AreaSqm { get; set; }
    public string? Location { get; set; }
    public string? Timeline { get; set; }
    public decimal? Budget { get; set; }

    // Calculator output (client-side estimate shown to user)
    public string? EstimateRange { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;
    public string? AdminNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
