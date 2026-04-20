namespace Avenue804.Web.Domain;

public enum NotificationType { NewInquiry, NewEmergencyTicket, NewAmcRequest, NewServiceQuote, NewUser, PriceDropAlert, NewReview }

public class AdminNotification
{
    public int Id { get; set; }
    public NotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
