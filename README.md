# 804 Avenue — Properties & Contracting

ASP.NET Core 8 Razor Pages site for 804 Avenue's real-estate, contracting,
maintenance and AMC operations. SQL Server (LocalDB / SQL Express) for storage,
ASP.NET Identity for auth, Bootstrap 5 + custom CSS for the front-end.

## Solution layout

| Folder                | Purpose                                            |
| --------------------- | -------------------------------------------------- |
| `src/Avenue804.Web`   | Main web app (Razor Pages, Admin area, services)   |
| `tools/`              | Misc Python helpers used during the legacy import  |
| `assets/`, `pages/`   | Legacy static HTML used as visual reference        |
| `index.html`          | Original static landing page (kept for reference)  |

## Prerequisites

- .NET SDK 8.x
- SQL Server (LocalDB, Express, or full instance)
- Node is **not** required — front-end assets are committed under
  `wwwroot/assets/`

## Getting started

```powershell
dotnet restore
dotnet ef database update --project src/Avenue804.Web
dotnet run --project src/Avenue804.Web
```

The admin user is seeded at startup using `Seed:AdminEmail` /
`Seed:AdminPassword` (development defaults are in
`appsettings.Development.json`). Sign in at `/Account/Login` or hit the
admin area directly at `/Admin`.

## Configuration & secrets

All secret values live **outside** of `appsettings.json`. The repo only
contains empty placeholders:

| Section                     | Owner / billing                                   |
| --------------------------- | ------------------------------------------------- |
| `Auth:Google`               | Google Cloud OAuth client                         |
| `Auth:Facebook`             | Meta for Developers                               |
| `Auth:Apple`                | Apple Developer Program                           |
| `Auth:Twilio`               | Twilio Programmable Messaging (OTP login)         |
| `Smtp`                      | Outbound email (MailKit)                          |
| `Storage:AzureConnectionString` | Azure Blob Storage (image uploads)            |
| `Maps:GoogleMapsApiKey`     | Google Maps JavaScript API                        |
| `Analytics:GA4MeasurementId`| Google Analytics 4                                |
| `LiveChat:Tawkto*`          | Tawk.to widget                                    |
| `Recaptcha`                 | Google reCAPTCHA v3 (form spam protection)        |

### Local development — `dotnet user-secrets`

The user-secrets store keeps secrets on the developer's machine, never in
source control. The project is already initialised (see
`Avenue804.Web.csproj`'s `<UserSecretsId>`); you just need to set values:

```powershell
cd src/Avenue804.Web

# Outbound email
dotnet user-secrets set "Smtp:Host"        "smtp.gmail.com"
dotnet user-secrets set "Smtp:User"        "your.dev@gmail.com"
dotnet user-secrets set "Smtp:Password"    "<google app password>"

# Twilio OTP
dotnet user-secrets set "Auth:Twilio:AccountSid" "ACxxxxxxxxxxxx"
dotnet user-secrets set "Auth:Twilio:AuthToken"  "xxxxxxxxxxxx"
dotnet user-secrets set "Auth:Twilio:FromNumber" "+1XXXXXXXXXX"

# OAuth (only if you want to test social login locally)
dotnet user-secrets set "Auth:Google:ClientId"     "xxxxx.apps.googleusercontent.com"
dotnet user-secrets set "Auth:Google:ClientSecret" "xxxxxxxxxxxx"

# Maps / reCAPTCHA / GA4
dotnet user-secrets set "Maps:GoogleMapsApiKey"        "xxxxx"
dotnet user-secrets set "Recaptcha:SiteKey"            "6Lxxxxxxx"
dotnet user-secrets set "Recaptcha:SecretKey"          "6Lxxxxxxx"
dotnet user-secrets set "Analytics:GA4MeasurementId"   "G-XXXXXXX"

# List everything
dotnet user-secrets list
```

### Production

In production, override the same keys through environment variables, Azure
App Service application settings, Key Vault references, or whichever secret
store you prefer. The double-underscore convention applies, e.g.
`Auth__Twilio__AccountSid`.

If a secret is empty at startup, the corresponding feature is silently
skipped (e.g. Apple Sign-In is only registered when all four credentials are
present, OTP send returns `false` when Twilio is not configured, etc.).

## Feature flags

Runtime feature toggles live in the `SiteSettings` table with key prefix
`feature.` and are managed at `/Admin/Features` (no redeploy required). See
`Services/IFeatureFlagService.cs` for the canonical list. Notable flags:

- `user_email_confirm` — when enabled, new accounts must verify their email
  before they can sign in.
- `recaptcha_v3` — when enabled, public submission forms must include a valid
  reCAPTCHA v3 token (requires `Recaptcha:SiteKey` / `SecretKey`).
- `phone_otp`, `google_signin`, `facebook_signin`, `apple_signin` — gate the
  optional sign-in methods (also require their underlying credentials).

## Security headers & CSP

`SecurityHeadersMiddleware` (in `Middleware/`) emits a per-request CSP
nonce on every HTML response together with the standard hardening headers
(`X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`,
`X-Frame-Options`). Inline `<script>` blocks must include
`nonce="@HttpContextAccessor.HttpContext.GetCspNonce()"`; prefer external
scripts under `wwwroot/assets/js/`.

The CSP is emitted in **Report-Only** mode by default so legacy inline
event handlers (`onclick=`, `onchange=`) keep working while the codebase
is incrementally migrated to nonce'd or external scripts. Switch to
enforcement once the console reports are clean by setting
`Security:CspReportOnly = "false"`. Set `Security:DisableCsp = "true"`
to disable the header entirely (debugging only).

## Rate limiting

The built-in .NET 8 `RateLimiter` is wired up in `Program.cs` with three
policies:

| Policy      | Window     | Limit                  | Applied to                              |
| ----------- | ---------- | ---------------------- | --------------------------------------- |
| `submit`    | 1 minute   | 10 requests / IP       | All `/Submit/*` endpoints               |
| `otp-send`  | 5 minutes  | 3 requests / IP+phone  | `/Account/Login?handler=SendOtp`        |
| `login`     | 5 minutes  | 8 requests / IP        | `/Account/Login?handler=Email`          |

## Tests

A separate `tests/Avenue804.Web.Tests` project lands in Phase 7 of the
audit-fix plan (xUnit + `WebApplicationFactory`). Until then, `dotnet build`
in the repo root is the smoke test.

## Audit & roadmap

The full multi-phase remediation plan lives at
`.cursor/plans/full-audit-fix-plan_06c6aa6e.plan.md`. Phase 0 (security &
hygiene) is shipped; later phases (service-first repositioning, conversion
forms, admin restructure, property perf, SEO, a11y/Arabic, tests) are
sequenced in that document.
