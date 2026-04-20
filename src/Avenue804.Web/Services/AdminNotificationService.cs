using Avenue804.Web.Data;
using Avenue804.Web.Domain;

namespace Avenue804.Web.Services;

public class AdminNotificationService : IAdminNotificationService
{
    private readonly ApplicationDbContext _db;
    public AdminNotificationService(ApplicationDbContext db) => _db = db;

    public async Task CreateAsync(NotificationType type, string message, string? linkUrl = null, CancellationToken ct = default)
    {
        _db.AdminNotifications.Add(new AdminNotification
        {
            Type = type,
            Message = message,
            LinkUrl = linkUrl
        });
        await _db.SaveChangesAsync(ct);
    }
}
