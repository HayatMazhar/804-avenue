using System.Text;
using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Infrastructure;
using Avenue804.Web.Middleware;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.Configure<SiteOptions>(builder.Configuration.GetSection(SiteOptions.SectionName));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ISiteBrandingService, SiteBrandingService>();
builder.Services.AddScoped<ILookupService, LookupService>();
builder.Services.AddScoped<IContentBlockService, ContentBlockService>();
builder.Services.AddScoped<IInquirySubmitter, InquirySubmitter>();
builder.Services.AddScoped<IPropertyListingInquirySubmitter, PropertyListingInquirySubmitter>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 10;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/Login";
    options.LogoutPath = "/Admin/Logout";
    options.AccessDeniedPath = "/Admin/Login";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole(SeedData.AdminRole));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
    options.Conventions.AllowAnonymousToAreaPage("Admin", "/Login");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Error", "?code={0}");

app.UseRouting();

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
