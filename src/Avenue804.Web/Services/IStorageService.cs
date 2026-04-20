namespace Avenue804.Web.Services;

/// <summary>
/// Abstracts file storage — switch between Local disk and Azure Blob via Storage:Provider config.
/// </summary>
public interface IStorageService
{
    /// <summary>Uploads a file and returns its public URL.</summary>
    Task<string> UploadAsync(IFormFile file, string folder = "listings", CancellationToken ct = default);

    /// <summary>Deletes a file by its URL. No-op if not found.</summary>
    Task DeleteAsync(string? url, CancellationToken ct = default);

    /// <summary>Returns the base URL for uploaded files.</summary>
    string BaseUrl { get; }
}
