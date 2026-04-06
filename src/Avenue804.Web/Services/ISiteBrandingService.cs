namespace Avenue804.Web.Services;

public interface ISiteBrandingService
{
    Task<SiteBrandingSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);

    void InvalidateCache();
}
