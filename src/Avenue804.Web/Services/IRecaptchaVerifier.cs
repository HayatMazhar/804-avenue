namespace Avenue804.Web.Services;

/// <summary>
/// Verifies a Google reCAPTCHA v3 token against Google's siteverify endpoint.
/// When the <c>recaptcha_v3</c> feature flag is OFF or no <c>Recaptcha:SecretKey</c>
/// is configured, <see cref="VerifyAsync"/> returns <c>true</c> so the rest of the
/// app continues to work in dev / when reCAPTCHA is intentionally disabled.
/// </summary>
public interface IRecaptchaVerifier
{
    /// <summary>True when reCAPTCHA enforcement is enabled (feature flag + secret).</summary>
    Task<bool> IsEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// Verifies the supplied token. Returns true when:
    /// <list type="bullet">
    /// <item>reCAPTCHA is disabled (flag off, no secret), OR</item>
    /// <item>Google returns <c>success: true</c> with a score &gt;= the configured minimum AND the action matches.</item>
    /// </list>
    /// </summary>
    Task<bool> VerifyAsync(string? token, string expectedAction, CancellationToken ct = default);
}
