namespace Avenue804.Web.Services;

/// <summary>
/// Runtime feature switches — values are stored in SiteSettings with key prefix "feature.".
/// Admins can toggle them from /Admin/Features without redeployment.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>Returns true when the flag is explicitly "true" in the database.</summary>
    Task<bool> IsEnabledAsync(string flag, CancellationToken cancellationToken = default);

    /// <summary>Returns all flags and their current state.</summary>
    Task<IReadOnlyDictionary<string, bool>> GetAllAsync(CancellationToken cancellationToken = default);

    void InvalidateCache();
}

/// <summary>Well-known flag names — use these constants everywhere.</summary>
public static class FeatureFlags
{
    // ── Social / Auth (paid API keys required) ──────────────────────
    public const string GoogleSignIn    = "google_signin";
    public const string FacebookSignIn  = "facebook_signin";
    public const string AppleSignIn     = "apple_signin";
    public const string PhoneOtp        = "phone_otp";

    // ── Communication (SMTP billing) ────────────────────────────────
    public const string EmailNotifications = "email_notifications";
    public const string UserEmailConfirm   = "user_email_confirm";

    // ── Maps / Location (Google billing) ────────────────────────────
    public const string GoogleMaps = "google_maps";

    // ── UI / Engagement features ────────────────────────────────────
    public const string WhatsAppFloat     = "whatsapp_float";
    public const string MortgageCalc      = "mortgage_calc";
    public const string PropertyRatings   = "property_ratings";
    public const string PropertyCompare   = "property_compare";
    public const string CurrencyConverter = "currency_converter";

    public static string DbKey(string flag) => $"feature.{flag}";

    public static IReadOnlyList<(string Flag, string Label, string Description, bool DefaultOn)> All =>
    [
        (GoogleSignIn,       "Google Sign-In",           "Allow users to sign in with their Google account.",                        false),
        (FacebookSignIn,     "Facebook Sign-In",         "Allow users to sign in with their Facebook account.",                      false),
        (AppleSignIn,        "Apple Sign-In",            "Allow users to sign in with their Apple ID.",                              false),
        (PhoneOtp,           "Phone OTP Login",          "Allow users to sign in by receiving an SMS code (Twilio billing).",        false),
        (EmailNotifications, "Admin Email Notifications","Email the admin team when new inquiries or listing requests are received.", false),
        (UserEmailConfirm,   "User Email Verification",  "Send a verification email when a new account is created (SMTP required).", false),
        (GoogleMaps,         "Google Maps",              "Show an interactive map on the Properties page (Google billing).",         false),
        (WhatsAppFloat,      "WhatsApp Floating Button", "Show the WhatsApp chat bubble on every page.",                            true),
        (MortgageCalc,       "Mortgage Calculator",      "Show the mortgage/rent calculator widget on property detail pages.",       true),
        (PropertyRatings,    "Property Ratings",         "Allow signed-in users to rate and review property listings.",              true),
        (PropertyCompare,    "Property Comparison",      "Allow users to compare up to 3 properties side by side.",                 true),
        (CurrencyConverter,  "Currency Converter",       "Show a currency toggle (AED / USD / EUR / GBP) on property pages.",       true),
    ];
}
