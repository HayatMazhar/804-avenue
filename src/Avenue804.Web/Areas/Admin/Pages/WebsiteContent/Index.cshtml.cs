using Avenue804.Web.Configuration;
using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Areas.Admin.Pages.WebsiteContent;

[Authorize(Policy = "AdminOnly")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public IndexModel(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public Dictionary<string, string> BlockBodies { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, SiteSetting> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public string? SuccessMessage { get; set; }

    public record ContentField(string Slug, string Label, bool IsTextarea = false);

    public static readonly List<(string Group, List<ContentField> Fields)> ContentSections =
    [
        ("Home Page", [
            new(ContentBlockSlugs.HomeHeroEyebrow, "Hero Eyebrow"),
            new(ContentBlockSlugs.HomeHeroHeadline, "Hero Headline"),
            new(ContentBlockSlugs.HomeHeroSubtitle, "Hero Subtitle"),
            new(ContentBlockSlugs.HomeCtaTitle, "CTA Banner Title"),
            new(ContentBlockSlugs.HomeAboutTag, "About Section Tag"),
            new(ContentBlockSlugs.HomeAboutTitle, "About Section Title"),
            new(ContentBlockSlugs.HomeTestiTag, "Testimonials Tag"),
            new(ContentBlockSlugs.HomeTestiTitle, "Testimonials Title"),
        ]),
        ("About Page", [
            new(ContentBlockSlugs.PageAboutHeroTitle, "Hero Title"),
            new(ContentBlockSlugs.PageAboutHeroSubtitle, "Hero Subtitle"),
            new(ContentBlockSlugs.AboutStripLead, "Introduction Text", true),
            new(ContentBlockSlugs.AboutStoryTitle, "Story Title"),
            new(ContentBlockSlugs.AboutStoryBody, "Story Body", true),
            new(ContentBlockSlugs.AboutVisionBody, "Vision Statement", true),
            new(ContentBlockSlugs.AboutMissionBody, "Mission Statement", true),
        ]),
        ("Contracting Page", [
            new(ContentBlockSlugs.PageContractingHeroTitle, "Hero Title"),
            new(ContentBlockSlugs.PageContractingHeroSubtitle, "Hero Subtitle"),
            new(ContentBlockSlugs.ContractingProcessTitle, "Process Section Title"),
            new(ContentBlockSlugs.ContractingCtaTitle, "CTA Title"),
            new(ContentBlockSlugs.ContractingCtaBody, "CTA Body", true),
        ]),
        ("Maintenance Page", [
            new(ContentBlockSlugs.PageMaintenanceHeroTitle, "Hero Title"),
            new(ContentBlockSlugs.PageMaintenanceHeroSubtitle, "Hero Subtitle"),
            new(ContentBlockSlugs.MaintenanceContractTitle, "Contract Section Title"),
            new(ContentBlockSlugs.MaintenanceContractBody, "Contract Body", true),
            new(ContentBlockSlugs.MaintenanceCtaTitle, "CTA Title"),
            new(ContentBlockSlugs.MaintenanceCtaBody, "CTA Body", true),
        ]),
        ("Facility Management Page", [
            new(ContentBlockSlugs.PageFacilityHeroTitle, "Hero Title"),
            new(ContentBlockSlugs.PageFacilityHeroSubtitle, "Hero Subtitle"),
            new(ContentBlockSlugs.FacilityWhyTitle, "Why FM Title"),
            new(ContentBlockSlugs.FacilityCtaTitle, "CTA Title"),
            new(ContentBlockSlugs.FacilityCtaBody, "CTA Body", true),
        ]),
    ];

    public static readonly List<(string Key, string Label)> BrandingFields =
    [
        (SiteSettingKeys.CompanyDisplayName, "Company Name"),
        (SiteSettingKeys.NavTagline, "Navigation Tagline"),
        (SiteSettingKeys.FooterTagline, "Footer Tagline"),
        (SiteSettingKeys.PhoneDisplay, "Phone (Display)"),
        (SiteSettingKeys.PhoneE164, "Phone (E.164)"),
        (SiteSettingKeys.Email, "Email Address"),
        (SiteSettingKeys.Address, "Office Address"),
        (SiteSettingKeys.WhatsAppDigits, "WhatsApp Number"),
        (SiteSettingKeys.WebsiteUrl, "Website URL"),
        (SiteSettingKeys.InstagramUrl, "Instagram URL"),
        (SiteSettingKeys.LinkedInUrl, "LinkedIn URL"),
        (SiteSettingKeys.FacebookUrl, "Facebook URL"),
        (SiteSettingKeys.CopyrightOwner, "Copyright Owner"),
        (SiteSettingKeys.DefaultMetaDescription, "Default Meta Description"),
        (SiteSettingKeys.DefaultListingCurrency, "Default Currency"),
    ];

    public async Task OnGetAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        SuccessMessage = TempData["ToastOk"] as string;
        await LoadDataAsync(ct);
    }

    public async Task<IActionResult> OnPostContentAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        var form = Request.Form;
        int updated = 0;

        var allBlocks = await _db.ContentBlocks.ToListAsync(ct);
        var bySlug = allBlocks.ToDictionary(b => b.Slug, StringComparer.OrdinalIgnoreCase);

        foreach (var section in ContentSections)
        {
            foreach (var field in section.Fields)
            {
                var formKey = "cb_" + field.Slug.Replace('.', '_');
                if (!form.ContainsKey(formKey)) continue;

                var newBody = form[formKey].ToString().Trim();
                if (bySlug.TryGetValue(field.Slug, out var existing))
                {
                    if (existing.Body != newBody)
                    {
                        ContentBlockService.Invalidate(_cache, existing.Slug);
                        existing.Body = newBody;
                        existing.UpdatedAt = DateTimeOffset.UtcNow;
                        updated++;
                    }
                }
                else if (!string.IsNullOrEmpty(newBody))
                {
                    _db.ContentBlocks.Add(new ContentBlock
                    {
                        Slug = field.Slug,
                        Body = newBody,
                        IsPublished = true,
                        UpdatedAt = DateTimeOffset.UtcNow
                    });
                    updated++;
                }
            }
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
            TempData["ToastOk"] = $"{updated} content block(s) saved.";
        }
        else
        {
            TempData["ToastOk"] = "No changes detected.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBrandingAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        var form = Request.Form;
        int updated = 0;

        var allSettings = await _db.SiteSettings.ToListAsync(ct);
        var byKey = allSettings.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, _) in BrandingFields)
        {
            var formKey = "ss_" + key;
            if (!form.ContainsKey(formKey)) continue;

            var newVal = form[formKey].ToString().Trim();
            if (byKey.TryGetValue(key, out var existing))
            {
                if (existing.Value != newVal)
                {
                    existing.Value = newVal;
                    existing.UpdatedAt = DateTimeOffset.UtcNow;
                    updated++;
                }
            }
            else if (!string.IsNullOrEmpty(newVal))
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = newVal,
                    UpdatedAt = DateTimeOffset.UtcNow
                });
                updated++;
            }
        }

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
            _cache.Remove("site-branding-snapshot-v1");
            TempData["ToastOk"] = $"{updated} setting(s) saved.";
        }
        else
        {
            TempData["ToastOk"] = "No changes detected.";
        }

        return RedirectToPage();
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        var blocks = await _db.ContentBlocks.AsNoTracking().ToListAsync(ct);
        BlockBodies = blocks.ToDictionary(b => b.Slug, b => b.Body, StringComparer.OrdinalIgnoreCase);

        var settings = await _db.SiteSettings.AsNoTracking().ToListAsync(ct);
        Settings = settings.ToDictionary(s => s.Key, s => s, StringComparer.OrdinalIgnoreCase);
    }
}
