namespace Avenue804.Web.Services;

/// <summary>
/// Defence-in-depth validation for user-uploaded image files. Combines
/// extension, declared MIME, file-size and magic-byte sniffing to catch the
/// usual "rename evil.exe to evil.jpg" trick. Does NOT touch the file content
/// (no re-encoding / EXIF stripping yet — see TODO).
/// </summary>
public static class FileUploadValidator
{
    /// <summary>10 MB upper bound for image uploads.</summary>
    public const long DefaultMaxBytes = 10L * 1024 * 1024;

    public static readonly IReadOnlySet<string> AllowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    public static readonly IReadOnlySet<string> AllowedImageMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif"
    };

    public readonly record struct ValidationResult(bool Ok, string? Error)
    {
        public static ValidationResult Success { get; } = new(true, null);
        public static ValidationResult Fail(string error) => new(false, error);
    }

    /// <summary>
    /// Runs the full image-upload validation pipeline. Reads the first ~12 bytes
    /// of the stream to sniff magic bytes; the stream is rewound to position 0
    /// before returning so callers can re-read it.
    /// </summary>
    public static async Task<ValidationResult> ValidateImageAsync(IFormFile file, long maxBytes = DefaultMaxBytes, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return ValidationResult.Fail("No file provided.");

        if (file.Length > maxBytes)
            return ValidationResult.Fail($"File too large. Maximum size is {maxBytes / (1024 * 1024)} MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return ValidationResult.Fail($"File extension '{ext}' is not allowed. Use JPG, PNG, WebP or GIF.");

        if (!string.IsNullOrEmpty(file.ContentType) && !AllowedImageMimeTypes.Contains(file.ContentType))
            return ValidationResult.Fail($"Content type '{file.ContentType}' is not allowed.");

        // Magic-byte sniff
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length), ct);
        if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

        if (read < 4 || !LooksLikeImage(header.AsSpan(0, read), ext))
            return ValidationResult.Fail("File does not appear to be a valid image.");

        return ValidationResult.Success;
    }

    /// <summary>Random GUID-based blob name with the original (lowercased) extension.</summary>
    public static string GenerateBlobName(string originalFileName)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext)) ext = ".bin";
        return $"{Guid.NewGuid():N}{ext}";
    }

    private static bool LooksLikeImage(ReadOnlySpan<byte> header, string ext)
    {
        // JPEG: FF D8 FF
        if (header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return ext is ".jpg" or ".jpeg";

        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header.Length >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return ext is ".png";

        // GIF: "GIF87a" / "GIF89a"
        if (header.Length >= 6 && header[0] == 'G' && header[1] == 'I' && header[2] == 'F' && header[3] == '8'
            && (header[4] == '7' || header[4] == '9') && header[5] == 'a')
            return ext is ".gif";

        // WebP: "RIFF????WEBP"
        if (header.Length >= 12 && header[0] == 'R' && header[1] == 'I' && header[2] == 'F' && header[3] == 'F'
            && header[8] == 'W' && header[9] == 'E' && header[10] == 'B' && header[11] == 'P')
            return ext is ".webp";

        return false;
    }
}
