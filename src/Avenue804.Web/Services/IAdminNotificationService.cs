using Avenue804.Web.Domain;

namespace Avenue804.Web.Services;

public interface IAdminNotificationService
{
    Task CreateAsync(NotificationType type, string message, string? linkUrl = null, CancellationToken ct = default);
}
