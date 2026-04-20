namespace Avenue804.Web.Domain;

public class ServiceReport
{
    public int Id { get; set; }
    public int? TicketId { get; set; }
    public MaintenanceTicket? Ticket { get; set; }
    public string ClientEmail { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReportFileUrl { get; set; }   // URL to PDF (uploaded externally or Blob storage)
    public DateTimeOffset VisitDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
