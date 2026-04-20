namespace Avenue804.Web.Domain;

public enum TicketPriority { Normal, High, Emergency }

public enum TicketStatus { New, Assigned, InProgress, Resolved, Closed }

public class MaintenanceTicket
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string BuildingOrLocation { get; set; } = string.Empty;
    public string IssueDescription { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Normal;
    public TicketStatus Status { get; set; } = TicketStatus.New;

    public bool IsEmergency => Priority == TicketPriority.Emergency;

    public string? AdminNotes { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
}
