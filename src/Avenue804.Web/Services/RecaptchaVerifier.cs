using System.Globalization;
using System.Net.Http.Json;

namespace Avenue804.Web.Services;

public sealed class RecaptchaVerifier : IRecaptchaVerifier
{
    private const string VerifyEndpoint = "https://www.google.com/recaptcha/api/siteverify";

    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _cfg;
    private readonly IFeatureFlagService _flags;
    private readonly ILogger<RecaptchaVerifier> _log;

    public RecaptchaVerifier(IHttpClientFactory http, IConfiguration cfg, IFeatureFlagService flags, ILogger<RecaptchaVerifier> log)
    {
        _http = http;
        _cfg = cfg;
        _flags = flags;
        _log = log;
    }

    public async Task<bool> IsEnabledAsync(CancellationToken ct = default)
    {
        var secret = _cfg["Recaptcha:SecretKey"];
        if (string.IsNullOrWhiteSpace(secret)) return false;
        return await _flags.IsEnabledAsync(FeatureFlags.RecaptchaV3, ct);
    }

    public async Task<bool> VerifyAsync(string? token, string expectedAction, CancellationToken ct = default)
    {
        if (!await IsEnabledAsync(ct)) return true;
        if (string.IsNullOrWhiteSpace(token))
        {
            _log.LogWarning("reCAPTCHA token missing for action {Action}", expectedAction);
            return false;
        }

        var secret = _cfg["Recaptcha:SecretKey"]!;
        var minScore = double.TryParse(_cfg["Recaptcha:MinScore"], NumberStyles.Float, CultureInfo.InvariantCulture, out var s) ? s : 0.5;

        try
        {
            var client = _http.CreateClient(nameof(RecaptchaVerifier));
            client.Timeout = TimeSpan.FromSeconds(5);

            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = secret,
                ["response"] = token
            });
            using var resp = await client.PostAsync(VerifyEndpoint, content, ct);
            if (!resp.IsSuccessStatusCode)
            {
                _log.LogWarning("reCAPTCHA siteverify HTTP {Status}", resp.StatusCode);
                return false;
            }

            var body = await resp.Content.ReadFromJsonAsync<SiteVerifyResponse>(cancellationToken: ct);
            if (body is null || !body.Success)
            {
                _log.LogWarning("reCAPTCHA verify failed: success={Success}, errors={Errors}", body?.Success, body?.ErrorCodes is null ? "" : string.Join(",", body.ErrorCodes));
                return false;
            }

            if (!string.IsNullOrEmpty(expectedAction) && !string.Equals(body.Action, expectedAction, StringComparison.OrdinalIgnoreCase))
            {
                _log.LogWarning("reCAPTCHA action mismatch: expected={Expected}, got={Actual}", expectedAction, body.Action);
                return false;
            }

            if (body.Score < minScore)
            {
                _log.LogWarning("reCAPTCHA low score {Score} (min {Min}) for action {Action}", body.Score, minScore, expectedAction);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "reCAPTCHA verify threw — failing closed.");
            return false;
        }
    }

    private sealed class SiteVerifyResponse
    {
        public bool Success { get; set; }
        public double Score { get; set; }
        public string? Action { get; set; }
        public string? Hostname { get; set; }
        public DateTimeOffset? ChallengeTs { get; set; }
        public string[]? ErrorCodes { get; set; }
    }
}
