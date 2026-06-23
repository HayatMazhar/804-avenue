using Microsoft.AspNetCore.Hosting;

namespace Avenue804.Web.Services;

/// <summary>
/// Saves uploaded files to wwwroot/uploads/{folder}/ and serves via /uploads/{folder}/{filename}.
/// Set Storage:Provider = "local" in appsettings.
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _http;
    private readonly string _uploadRoot;
    private readonly ILogger<LocalStorageService> _log;

    public LocalStorageService(IWebHostEnvironment env, IHttpContextAccessor http, ILogger<LocalStorageService> log)
    {
        _env = env;
        _http = http;
        _log = log;
        _uploadRoot = Path.Combine(env.WebRootPath, "uploads");

        // Ensure the uploads root exists at startup; log a clear error if the
        // directory can't be created (e.g. read-only hosting) rather than
        // failing silently on the first upload attempt.
        try
        {
            Directory.CreateDirectory(_uploadRoot);
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "LocalStorageService: cannot create uploads directory at {Path}. " +
                "Photo uploads will fail until this is resolved. " +
                "Check that the application has write permission to wwwroot/uploads.",
                _uploadRoot);
        }
    }

    public string BaseUrl
    {
        get
        {
            var req = _http.HttpContext?.Request;
            return req != null ? $"{req.Scheme}://{req.Host}" : "";
        }
    }

    public async Task<string> UploadAsync(IFormFile file, string folder = "listings", CancellationToken ct = default)
    {
        var validation = await FileUploadValidator.ValidateImageAsync(file, ct: ct);
        if (!validation.Ok)
            throw new InvalidOperationException(validation.Error);

        var safeFolder = SanitiseFolder(folder);
        var dir = Path.Combine(_uploadRoot, safeFolder);

        try
        {
            Directory.CreateDirectory(dir);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Cannot create upload directory {Dir}", dir);
            throw new InvalidOperationException(
                "The server could not create the upload folder. " +
                "Please check that wwwroot/uploads is writable on the hosting environment.", ex);
        }

        var fileName = FileUploadValidator.GenerateBlobName(file.FileName);
        var filePath = Path.Combine(dir, fileName);

        try
        {
            await using var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                bufferSize: 81920, useAsync: true);
            await file.CopyToAsync(stream, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Failed to write uploaded file to {Path}", filePath);
            throw new InvalidOperationException(
                "The server could not save the uploaded file. " +
                "Please check disk space and write permissions on wwwroot/uploads.", ex);
        }

        _log.LogInformation("Uploaded {Name} ({Bytes} bytes) → {Path}", file.FileName, file.Length, filePath);
        // Root-relative URL — works regardless of host, domain or scheme.
        return $"/uploads/{safeFolder}/{fileName}";
    }

    private static string SanitiseFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder)) return "listings";
        var clean = new string(folder.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        return string.IsNullOrEmpty(clean) ? "listings" : clean.ToLowerInvariant();
    }

    public Task DeleteAsync(string? url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return Task.CompletedTask;
        try
        {
            // Handles both root-relative ("/uploads/listings/x.jpg") and legacy
            // absolute ("https://host/uploads/listings/x.jpg") stored URLs.
            var absolutePath = Uri.TryCreate(url, UriKind.Absolute, out var abs)
                ? abs.AbsolutePath
                : url;
            var relativePath = absolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_env.WebRootPath, relativePath);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch (Exception ex)
        {
            _log.LogWarning("Could not delete local file {Url}: {Ex}", url, ex.Message);
        }
        return Task.CompletedTask;
    }

}
