using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Avenue804.Web.Pages.Api;

/// <summary>
/// Accepts a file upload and returns the public URL.
/// Called via AJAX from admin listing edit — supports multi-image upload.
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class UploadImageModel : PageModel
{
    private readonly IStorageService _storage;
    private readonly ILogger<UploadImageModel> _log;

    public UploadImageModel(IStorageService storage, ILogger<UploadImageModel> log)
    {
        _storage = storage;
        _log = log;
    }

    public IActionResult OnGet() => Forbid();

    public async Task<IActionResult> OnPostAsync(IFormFile? file, string? folder, CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
            return new JsonResult(new { error = "No file provided." }) { StatusCode = 400 };

        try
        {
            var url = await _storage.UploadAsync(file, folder ?? "listings", ct);
            return new JsonResult(new { url, size = file.Length, name = file.FileName });
        }
        catch (InvalidOperationException ex)
        {
            return new JsonResult(new { error = ex.Message }) { StatusCode = 400 };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Image upload failed for {Name}", file.FileName);
            return new JsonResult(new { error = "Upload failed. Please try again." }) { StatusCode = 500 };
        }
    }
}
