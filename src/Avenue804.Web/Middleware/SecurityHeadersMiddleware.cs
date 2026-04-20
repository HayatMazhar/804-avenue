using System.Security.Cryptography;

namespace Avenue804.Web.Middleware;

/// <summary>
/// Emits a per-request CSP nonce + the standard hardening headers
/// (Content-Security-Policy, X-Content-Type-Options, Referrer-Policy,
/// Permissions-Policy, X-Frame-Options).
///
/// Inline script blocks must include <c>nonce="@HttpContextAccessor.HttpContext.GetCspNonce()"</c>
/// or be moved to <c>wwwroot/assets/js/</c>. External script hosts are whitelisted
/// here — extend the list when a new CDN is introduced.
/// </summary>
public static class SecurityHeadersMiddleware
{
    public const string NonceItemKey = "__csp_nonce";

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app, IConfiguration config)
    {
        var enableCsp = !string.Equals(config["Security:DisableCsp"], "true", StringComparison.OrdinalIgnoreCase);
        // Default to report-only so legacy inline event handlers (onclick=, onchange=, …)
        // keep working while the Phase 0 migration to nonce'd scripts is being completed.
        // Set Security:CspReportOnly = "false" to switch to enforcement mode.
        var reportOnly = !string.Equals(config["Security:CspReportOnly"], "false", StringComparison.OrdinalIgnoreCase);

        return app.Use(async (ctx, next) =>
        {
            var nonce = GenerateNonce();
            ctx.Items[NonceItemKey] = nonce;

            ctx.Response.OnStarting(() =>
            {
                var h = ctx.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "SAMEORIGIN";
                h["Referrer-Policy"] = "strict-origin-when-cross-origin";
                h["Permissions-Policy"] = "geolocation=(self), microphone=(), camera=(), payment=()";

                if (enableCsp && IsHtmlResponse(ctx))
                {
                    var csp = BuildCspHeader(nonce);
                    h[reportOnly ? "Content-Security-Policy-Report-Only" : "Content-Security-Policy"] = csp;
                }
                return Task.CompletedTask;
            });

            await next();
        });
    }

    private static bool IsHtmlResponse(HttpContext ctx)
    {
        var ct = ctx.Response.ContentType;
        return string.IsNullOrEmpty(ct) || ct.Contains("text/html", StringComparison.OrdinalIgnoreCase);
    }

    private static string GenerateNonce()
    {
        Span<byte> buf = stackalloc byte[16];
        RandomNumberGenerator.Fill(buf);
        return Convert.ToBase64String(buf);
    }

    private static string BuildCspHeader(string nonce) => string.Join("; ", new[]
    {
        "default-src 'self'",
        // Scripts: self + nonce'd inline + whitelisted CDNs / chat / analytics
        $"script-src 'self' 'nonce-{nonce}' 'strict-dynamic' https: 'unsafe-inline'",
        // 'unsafe-inline' above is ignored by browsers that honour the nonce; kept as a fallback for legacy browsers.
        // Styles: many third-party libs (Bootstrap, Tom-Select, Flatpickr) inject inline styles
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com",
        "style-src-elem 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://fonts.googleapis.com",
        "img-src 'self' data: blob: https:",
        "font-src 'self' data: https://fonts.gstatic.com https://cdn.jsdelivr.net",
        "connect-src 'self' https://www.google-analytics.com https://*.tawk.to wss://*.tawk.to https://www.google.com https://maps.googleapis.com",
        "frame-src 'self' https://www.google.com https://www.youtube.com https://www.youtube-nocookie.com",
        "object-src 'none'",
        "base-uri 'self'",
        "form-action 'self'",
        "frame-ancestors 'self'",
        "upgrade-insecure-requests"
    });
}

/// <summary>Helpers to read the per-request CSP nonce from views / pages.</summary>
public static class CspNonceExtensions
{
    /// <summary>Returns the per-request CSP nonce (or an empty string if no middleware is wired).</summary>
    public static string GetCspNonce(this HttpContext? ctx)
        => ctx?.Items[SecurityHeadersMiddleware.NonceItemKey] as string ?? string.Empty;
}
