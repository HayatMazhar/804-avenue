using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Middleware;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ISiteBrandingService, SiteBrandingService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IContentBlockService, ContentBlockService>();
builder.Services.AddScoped<IInquirySubmitter, InquirySubmitter>();
builder.Services.AddScoped<IPropertyListingInquirySubmitter, PropertyListingInquirySubmitter>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddScoped<ITwilioOtpService, TwilioOtpService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddScoped<IHomeStatsService, HomeStatsService>();
builder.Services.AddScoped<SavedSearchAlertService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRecaptchaVerifier, RecaptchaVerifier>();
builder.Services.AddHostedService<WeeklyDigestJob>();

// Storage service — local disk by default, Azure Blob when Provider = "azure"
var storageProvider = builder.Configuration["Storage:Provider"] ?? "local";
if (storageProvider.Equals("azure", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddScoped<IStorageService, AzureBlobStorageService>();
else
    builder.Services.AddScoped<IStorageService, LocalStorageService>();

// ── Database ──────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ── Identity ──────────────────────────────────────────────────
// RequireConfirmedEmail is config-flagged so the dev seeded admin can still log in
// without an SMTP round-trip. Production enables it via Identity__RequireConfirmedEmail=true.
var requireConfirmedEmail = string.Equals(builder.Configuration["Identity:RequireConfirmedEmail"], "true", StringComparison.OrdinalIgnoreCase);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = requireConfirmedEmail;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ── External OAuth — ONLY registered when credentials are non-empty ─────
// AddIdentity already called AddAuthentication internally. We get a builder
// to chain extra schemes. Providers with empty keys are silently skipped.
var authBuilder = new Microsoft.AspNetCore.Authentication.AuthenticationBuilder(builder.Services);

var googleId     = builder.Configuration["Auth:Google:ClientId"]?.Trim();
var googleSecret = builder.Configuration["Auth:Google:ClientSecret"]?.Trim();
if (!string.IsNullOrEmpty(googleId) && !string.IsNullOrEmpty(googleSecret))
{
    authBuilder.AddGoogle(o =>
    {
        o.ClientId     = googleId;
        o.ClientSecret = googleSecret;
        o.CallbackPath = "/signin-google";
        o.SaveTokens   = true;
    });
}

var fbAppId     = builder.Configuration["Auth:Facebook:AppId"]?.Trim();
var fbAppSecret = builder.Configuration["Auth:Facebook:AppSecret"]?.Trim();
if (!string.IsNullOrEmpty(fbAppId) && !string.IsNullOrEmpty(fbAppSecret))
{
    authBuilder.AddFacebook(o =>
    {
        o.AppId     = fbAppId;
        o.AppSecret = fbAppSecret;
        o.CallbackPath = "/signin-facebook";
        o.SaveTokens   = true;
    });
}

// Apple Sign-In requires credentials from developer.apple.com — skipped when not configured
// To enable: set Auth:Apple:ClientId, KeyId, TeamId, PrivateKey in appsettings
var appleClientId = builder.Configuration["Auth:Apple:ClientId"]?.Trim();
var appleKeyId    = builder.Configuration["Auth:Apple:KeyId"]?.Trim();
var appleTeamId   = builder.Configuration["Auth:Apple:TeamId"]?.Trim();
var appleKey      = builder.Configuration["Auth:Apple:PrivateKey"]?.Trim();
if (!string.IsNullOrEmpty(appleClientId)
    && !string.IsNullOrEmpty(appleKeyId)
    && !string.IsNullOrEmpty(appleTeamId)
    && !string.IsNullOrEmpty(appleKey))
{
    // Only add Apple when ALL four credentials are present
    authBuilder.AddApple(o =>
    {
        o.ClientId   = appleClientId;
        o.KeyId      = appleKeyId;
        o.TeamId     = appleTeamId;
        o.CallbackPath       = "/signin-apple";
        o.SaveTokens         = true;
        o.GenerateClientSecret = true;
    });
}

// ── Cookie configuration (public users) ──────────────────────
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath      = "/Account/Login";
    options.LogoutPath     = "/Account/Logout";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.Cookie.Name      = ".804Avenue.Auth";
    options.Cookie.HttpOnly  = true;
    options.Cookie.SameSite  = SameSiteMode.Lax;
});

// ── Authorization policies ───────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",  p => p.RequireRole(SeedData.AdminRole));
    options.AddPolicy("PublicUser", p => p.RequireAuthenticatedUser());
});

// ── Razor Pages ───────────────────────────────────────────────
// AutoValidateAntiforgeryToken: every non-GET handler (POST/PUT/PATCH/DELETE)
// must include a valid antiforgery token. Razor Pages already does this for
// page handlers; the global filter enforces it for any plain controllers too.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AllowAnonymousToAreaPage("Admin", "/Login");
    options.Conventions.AuthorizePage("/Account/Dashboard", "PublicUser");
    options.Conventions.AuthorizePage("/Account/Profile",   "PublicUser");
    options.Conventions.AuthorizePage("/Account/Logout",    "PublicUser");
    options.Conventions.AuthorizePage("/AgentPortal/Dashboard", "PublicUser");
})
.AddMvcOptions(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// ── Rate limiting (built-in .NET 8) ──────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = (int)HttpStatusCode.TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "60";
        await context.HttpContext.Response.WriteAsync("Too many requests. Please slow down and try again.", ct);
    };

    static string ClientKey(HttpContext ctx)
        => ctx.Connection.RemoteIpAddress?.ToString() ?? "anon";

    // Public submission forms: 10 requests / minute per client IP
    options.AddPolicy("submit", ctx => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: ClientKey(ctx),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        }));

    // OTP send: 3 per 5 minutes per IP+phone
    options.AddPolicy("otp-send", ctx =>
    {
        var phone = ctx.Request.HasFormContentType
            ? ctx.Request.Form["phoneNumber"].ToString()
            : string.Empty;
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"{ClientKey(ctx)}|{phone}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0
            });
    });

    // Email login: 8 per 5 minutes per IP
    options.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: ClientKey(ctx),
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 8,
            Window = TimeSpan.FromMinutes(5),
            QueueLimit = 0
        }));
});

// ── Build & pipeline ─────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

app.UseSecurityHeaders(builder.Configuration);

app.UseRouting();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseSiteBranding();

app.MapRazorPages();

app.MapGet("/sitemap.xml", async (ApplicationDbContext db, HttpContext ctx, CancellationToken ct) =>
{
    var baseUrl = $"{ctx.Request.Scheme}://{ctx.Request.Host}";
    var xml = await SitemapBuilder.BuildAsync(db, baseUrl, ct);
    return Results.Text(xml, "application/xml; charset=utf-8", Encoding.UTF8);
});

await SeedData.EnsureSeededAsync(app);

app.Run();
