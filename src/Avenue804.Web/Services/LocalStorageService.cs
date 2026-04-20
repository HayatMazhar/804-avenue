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
        Directory.CreateDirectory(dir);

        var fileName = FileUploadValidator.GenerateBlobName(file.FileName);
        var filePath = Path.Combine(dir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream, ct);
        }

        _log.LogInformation("Uploaded file to local disk: {Path}", filePath);
        return $"{BaseUrl}/uploads/{safeFolder}/{fileName}";
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
            // Extract path from URL: /uploads/listings/filename.jpg
            var uri = new Uri(url);
            var relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
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
