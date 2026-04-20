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
        var validation = await FileUploadValidator.ValidateImageAsync(file, ct: ct);
        if (!validation.Ok)
            throw new InvalidOperationException(validation.Error);

        var containerClient = _client.GetBlobContainerClient(_container);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

        var safeFolder = SanitiseFolder(folder);
        var blobName = $"{safeFolder}/{FileUploadValidator.GenerateBlobName(file.FileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        // Trust our magic-byte sniff over the client-supplied Content-Type
        var ext = Path.GetExtension(blobName).ToLowerInvariant();
        var safeContentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            ".gif"            => "image/gif",
            _                  => "application/octet-stream"
        };

        var headers = new BlobHttpHeaders
        {
            ContentType = safeContentType,
            CacheControl = "public, max-age=31536000"
        };

        await using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = headers }, ct);

        var url = _cdnBaseUrl != null
            ? $"{_cdnBaseUrl}/{_container}/{blobName}"
            : blobClient.Uri.ToString();

        _log.LogInformation("Uploaded to Azure Blob: {Url}", url);
        return url;
    }

    private static string SanitiseFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return "listings";
        var clean = new string(folder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        return string.IsNullOrEmpty(clean) ? "listings" : clean.ToLowerInvariant();
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

}
