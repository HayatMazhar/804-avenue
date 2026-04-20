using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Avenue804.Web.Services;

/// <summary>
/// Uploads files to Azure Blob Storage and returns the CDN/blob URL.
/// Set Storage:Provider = "azure" and fill Storage:AzureConnectionString + Storage:ContainerName.
/// Optionally set Storage:CdnBaseUrl to override the blob URL with a CDN URL.
/// </summary>
public class AzureBlobStorageService : IStorageService
{
    private readonly BlobServiceClient _client;
    private readonly string _container;
    private readonly string? _cdnBaseUrl;
    private readonly ILogger<AzureBlobStorageService> _log;

    public AzureBlobStorageService(IConfiguration config, ILogger<AzureBlobStorageService> log)
    {
        _log = log;
        var connStr = config["Storage:AzureConnectionString"]
            ?? throw new InvalidOperationException("Storage:AzureConnectionString is required for Azure storage.");
        _container = config["Storage:ContainerName"] ?? "listings";
        _cdnBaseUrl = config["Storage:CdnBaseUrl"]?.TrimEnd('/');
        _client = new BlobServiceClient(connStr);
    }

    public string BaseUrl => _cdnBaseUrl ?? "";

    public async Task<string> UploadAsync(IFormFile file, string folder = "listings", CancellationToken ct = default)
    {
        var containerClient = _client.GetBlobContainerClient(_container);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!IsAllowedExtension(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed. Use JPG, PNG, or WebP.");

        var blobName = $"{folder}/{Guid.NewGuid():N}{ext}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var headers = new BlobHttpHeaders
        {
            ContentType = file.ContentType,
            CacheControl = "public, max-age=31536000"  // 1-year cache
        };

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, ct);

        var url = _cdnBaseUrl != null
            ? $"{_cdnBaseUrl}/{_container}/{blobName}"
            : blobClient.Uri.ToString();

        _log.LogInformation("Uploaded to Azure Blob: {Url}", url);
        return url;
    }

    public async Task DeleteAsync(string? url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        try
        {
            // Extract blob name from URL
            var uri = new Uri(url);
            var path = uri.AbsolutePath.TrimStart('/');
            // Remove container prefix if present
            var blobName = path.StartsWith(_container + "/") ? path[(_container.Length + 1)..] : path;
            var blobClient = _client.GetBlobContainerClient(_container).GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _log.LogWarning("Could not delete Azure blob {Url}: {Ex}", url, ex.Message);
        }
    }

    private static bool IsAllowedExtension(string ext)
        => ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif";
}
