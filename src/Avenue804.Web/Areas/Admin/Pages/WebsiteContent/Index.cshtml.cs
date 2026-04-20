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

/// <summary>
/// Page-by-page CMS editor. Every public-facing surface (Home, Services hub,
/// service detail pages, About, etc.) gets its own tab containing every
/// CMS-overridable slug that page actually consumes. Admins should never have
/// to look up a slug by name — they pick the page and edit each labelled field.
/// </summary>
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

    /// <summary>
    /// Content blocks present in the DB whose slug isn't currently wired to a
    /// page tab. Surfaced as a fallback "Other" tab so admins can edit
    /// everything, even legacy or experimental copy that hasn't been mapped to
    /// a public page yet.
    /// </summary>
    public List<ContentField> OrphanFields { get; set; } = [];

    public record ContentField(string Slug, string Label, bool IsTextarea = false, string? Hint = null);

    /// <summary>A logical "page" tab containing one or more editable fields.</summary>
    public record ContentPage(
        string Id,           // tab id (used in URL fragment)
        string Title,        // tab label
        string Description,  // shown above the form
        string? PreviewUrl,  // public URL to "Open page →"
        List<ContentField> Fields);

    /// <summary>
    /// Every public page → its CMS-overridable fields, in the order they appear
    /// on that page. Wire any new ContentBlockSlug here; the editor picks it up
    /// automatically, no markup changes required.
    /// </summary>
    public static readonly List<ContentPage> ContentPages =
    [
        new("page-home", "Home page",
            "Hero, service cards, stats, testimonials, CTA — the entire / landing page.",
            "/",
            [
                new(ContentBlockSlugs.HomeHeroEyebrow,    "Hero — eyebrow"),
                new(ContentBlockSlugs.HomeHeroTitle,      "Hero — title (HTML allowed)", true, "Use <em>…</em> for the orange accent."),
                new(ContentBlockSlugs.HomeHeroHeadline,   "Hero — headline (alt)"),
                new(ContentBlockSlugs.HomeHeroSubtitle,   "Hero — subtitle", true),
                new(ContentBlockSlugs.HomeHeroPropertiesLink, "Hero — properties link label"),
                new(ContentBlockSlugs.HomeServiceCardMaintLead, "Service card — Maintenance lead", true),
                new(ContentBlockSlugs.HomeServiceCardAmcLead,   "Service card — AMC lead", true),
                new(ContentBlockSlugs.HomeServiceCardContrLead, "Service card — Contracting lead", true),
                new(ContentBlockSlugs.HomeServiceCardFmLead,    "Service card — Facility Mgmt lead", true),
                new(ContentBlockSlugs.HomeAboutTag,       "About strip — tag"),
                new(ContentBlockSlugs.HomeAboutTitle,     "About strip — title"),
                new(ContentBlockSlugs.AboutStripLead,     "About strip — lead", true),
                new(ContentBlockSlugs.HomeAboutCards,     "About strip — cards (JSON)", true, "Optional structured JSON."),
                new(ContentBlockSlugs.HomeStatsItems,     "Stats strip — items (JSON)", true),
                new(ContentBlockSlugs.HomeReTag,          "Real estate strip — tag"),
                new(ContentBlockSlugs.HomeReTitle,        "Real estate strip — title"),
                new(ContentBlockSlugs.HomeReCards,        "Real estate strip — cards (JSON)", true),
                new(ContentBlockSlugs.HomeContractingTag, "Contracting strip — tag"),
                new(ContentBlockSlugs.HomeContractingTitle, "Contracting strip — title"),
                new(ContentBlockSlugs.HomeContractingLead,  "Contracting strip — lead", true),
                new(ContentBlockSlugs.HomeContractingCards, "Contracting strip — cards (JSON)", true),
                new(ContentBlockSlugs.HomeMaintenanceTag,   "Maintenance strip — tag"),
                new(ContentBlockSlugs.HomeMaintenanceTitle, "Maintenance strip — title"),
                new(ContentBlockSlugs.HomeMaintenanceLead,  "Maintenance strip — lead", true),
                new(ContentBlockSlugs.HomeMaintenanceCards, "Maintenance strip — cards (JSON)", true),
                new(ContentBlockSlugs.HomeWhyTag,    "Why-us strip — tag"),
                new(ContentBlockSlugs.HomeWhyTitle,  "Why-us strip — title"),
                new(ContentBlockSlugs.HomeWhyItems,  "Why-us strip — items (JSON)", true),
                new(ContentBlockSlugs.HomeTestiTag,   "Testimonials — tag"),
                new(ContentBlockSlugs.HomeTestiTitle, "Testimonials — title"),
                new(ContentBlockSlugs.HomeTestiItems, "Testimonials — items (JSON)", true),
                new(ContentBlockSlugs.HomeCtaTitle,   "Footer CTA — title"),
            ]),

        new("page-services", "Services hub",
            "The /Services landing page — hero, lead-in, and what every visitor sees first.",
            "/Services",
            [
                new(ContentBlockSlugs.PageServicesHubEyebrow,  "Hero — eyebrow"),
                new(ContentBlockSlugs.PageServicesHubTitle,    "Hero — title (HTML allowed)", true, "Use <em>…</em> for the orange accent."),
                new(ContentBlockSlugs.PageServicesHubSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.PageServicesHubLead,     "Cards section — lead", true),
            ]),

        new("page-about", "About",
            "Story, vision, mission, values, expertise and team copy on /About.",
            "/About",
            [
                new(ContentBlockSlugs.PageAboutHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageAboutHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.AboutStoryTag,    "Story section — tag"),
                new(ContentBlockSlugs.AboutStoryTitle,  "Story section — title"),
                new(ContentBlockSlugs.AboutStoryBody,   "Story section — body", true),
                new(ContentBlockSlugs.AboutVmTag,       "Vision/mission — tag"),
                new(ContentBlockSlugs.AboutVisionBody,  "Vision statement", true),
                new(ContentBlockSlugs.AboutMissionBody, "Mission statement", true),
                new(ContentBlockSlugs.AboutValuesTag,   "Values — tag"),
                new(ContentBlockSlugs.AboutValuesItems, "Values — items (JSON)", true),
                new(ContentBlockSlugs.AboutExpertiseTag,   "Expertise — tag"),
                new(ContentBlockSlugs.AboutExpertiseLead,  "Expertise — lead", true),
                new(ContentBlockSlugs.AboutExpertiseItems, "Expertise — items (JSON)", true),
                new(ContentBlockSlugs.AboutTeamLead,    "Team — lead", true),
                new(ContentBlockSlugs.AboutTeamItems,   "Team — items (JSON)", true),
                new(ContentBlockSlugs.AboutWcuTag,      "Why choose us — tag"),
                new(ContentBlockSlugs.AboutWcuItems,    "Why choose us — items (JSON)", true),
            ]),

        new("page-contracting", "Contracting",
            "Service detail page at /Contracting — hero, individual services, process and CTA.",
            "/Contracting",
            [
                new(ContentBlockSlugs.PageContractingHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageContractingHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.ContractingSvc1Title, "Service 1 — title"),
                new(ContentBlockSlugs.ContractingSvc1Body,  "Service 1 — body", true),
                new(ContentBlockSlugs.ContractingSvc2Title, "Service 2 — title"),
                new(ContentBlockSlugs.ContractingSvc2Body,  "Service 2 — body", true),
                new(ContentBlockSlugs.ContractingSvc3Title, "Service 3 — title"),
                new(ContentBlockSlugs.ContractingSvc3Body,  "Service 3 — body", true),
                new(ContentBlockSlugs.ContractingSvc4Title, "Service 4 — title"),
                new(ContentBlockSlugs.ContractingSvc4Body,  "Service 4 — body", true),
                new(ContentBlockSlugs.ContractingSvc5Title, "Service 5 — title"),
                new(ContentBlockSlugs.ContractingSvc5Body,  "Service 5 — body", true),
                new(ContentBlockSlugs.ContractingSvc6Title, "Service 6 — title"),
                new(ContentBlockSlugs.ContractingSvc6Body,  "Service 6 — body", true),
                new(ContentBlockSlugs.ContractingSvc7Title, "Service 7 — title"),
                new(ContentBlockSlugs.ContractingSvc7Body,  "Service 7 — body", true),
                new(ContentBlockSlugs.ContractingProcessTitle, "Process — title"),
                new(ContentBlockSlugs.ContractingProcessLead,  "Process — lead", true),
                new(ContentBlockSlugs.ContractingProcessSteps, "Process — steps (JSON)", true),
                new(ContentBlockSlugs.ContractingMaterialsItems, "Materials — items (JSON)", true),
                new(ContentBlockSlugs.ContractingCtaTitle, "CTA — title"),
                new(ContentBlockSlugs.ContractingCtaBody,  "CTA — body", true),
            ]),

        new("page-maintenance", "Maintenance",
            "Service detail page at /Maintenance — hero, individual services, AMC explainer and CTA.",
            "/Maintenance",
            [
                new(ContentBlockSlugs.PageMaintenanceHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageMaintenanceHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.MaintenanceSvc1Title, "Service 1 — title"),
                new(ContentBlockSlugs.MaintenanceSvc1Body,  "Service 1 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc2Title, "Service 2 — title"),
                new(ContentBlockSlugs.MaintenanceSvc2Body,  "Service 2 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc3Title, "Service 3 — title"),
                new(ContentBlockSlugs.MaintenanceSvc3Body,  "Service 3 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc4Title, "Service 4 — title"),
                new(ContentBlockSlugs.MaintenanceSvc4Body,  "Service 4 — body", true),
                new(ContentBlockSlugs.MaintenanceSvc5Title, "Service 5 — title"),
                new(ContentBlockSlugs.MaintenanceSvc5Body,  "Service 5 — body", true),
                new(ContentBlockSlugs.MaintenanceWhyItems,  "Why-us — items (JSON)", true),
                new(ContentBlockSlugs.MaintenanceContractTitle,    "AMC strip — title"),
                new(ContentBlockSlugs.MaintenanceContractBody,     "AMC strip — body", true),
                new(ContentBlockSlugs.MaintenanceContractFeatures, "AMC strip — features (JSON)", true),
                new(ContentBlockSlugs.MaintenanceCtaTitle, "CTA — title"),
                new(ContentBlockSlugs.MaintenanceCtaBody,  "CTA — body", true),
            ]),

        new("page-facility", "Facility Management",
            "Service detail page at /FacilityManagement — hero, services, why-FM and CTA.",
            "/FacilityManagement",
            [
                new(ContentBlockSlugs.PageFacilityHeroTitle,    "Hero — title"),
                new(ContentBlockSlugs.PageFacilityHeroSubtitle, "Hero — subtitle", true),
                new(ContentBlockSlugs.FacilitySvc1Title, "Service 1 — title"),
                new(ContentBlockSlugs.FacilitySvc1Body,  "Service 1 — body", true),
                new(ContentBlockSlugs.FacilitySvc2Title, "Service 2 — title"),
                new(ContentBlockSlugs.FacilitySvc2Body,  "Service 2 — body", true),
                new(ContentBlockSlugs.FacilityWhyTag,    "Why FM — tag"),
                new(ContentBlockSlugs.FacilityWhyTitle,  "Why FM — title"),
                new(ContentBlockSlugs.FacilityWhyItems,  "Why FM — items (JSON)", true),
                new(ContentBlockSlugs.FacilityCtaTitle,  "CTA — title"),
                new(ContentBlockSlugs.FacilityCtaBody,   "CTA — body", true),
            ]),
    ];

    /// <summary>Brand &amp; contact details that appear in nav, footer and metadata.</summary>
    public static readonly List<(string Key, string Label, string? Hint)> BrandingFields =
    [
        (SiteSettingKeys.CompanyDisplayName, "Company name", "e.g. 804 Avenue"),
        (SiteSettingKeys.NavTagline,         "Navigation tagline", "Shown next to the logo in the top nav."),
        (SiteSettingKeys.FooterTagline,      "Footer tagline", null),
        (SiteSettingKeys.PhoneDisplay,       "Phone (display)", "+971 50 43 99 804"),
        (SiteSettingKeys.PhoneE164,          "Phone (E.164)", "+971504399804 — used for tel: links."),
        (SiteSettingKeys.Email,              "Email address", null),
        (SiteSettingKeys.Address,            "Office address", null),
        (SiteSettingKeys.WhatsAppDigits,     "WhatsApp number", "Digits only, e.g. 971504399804"),
        (SiteSettingKeys.WebsiteUrl,         "Website URL", null),
        (SiteSettingKeys.InstagramUrl,       "Instagram URL", null),
        (SiteSettingKeys.LinkedInUrl,        "LinkedIn URL", null),
        (SiteSettingKeys.FacebookUrl,        "Facebook URL", null),
        (SiteSettingKeys.CopyrightOwner,     "Copyright owner", "Shown in the footer copyright notice."),
        (SiteSettingKeys.DefaultMetaDescription, "Default meta description", "Used on pages without their own description."),
        (SiteSettingKeys.DefaultListingCurrency, "Default currency", "AED, USD, EUR…"),
    ];

    public async Task OnGetAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        ViewData["Title"] = "Page Content";
        SuccessMessage = TempData["ToastOk"] as string;
        await LoadDataAsync(ct);
    }

    public async Task<IActionResult> OnPostContentAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        var form = Request.Form;
        var pageId = form["__page"].ToString();
        int updated = 0;

        var allBlocks = await _db.ContentBlocks.ToListAsync(ct);
        var bySlug = allBlocks.ToDictionary(b => b.Slug, StringComparer.OrdinalIgnoreCase);

        IEnumerable<ContentField> fieldsToUpdate;
        if (pageId == "page-other")
        {
            // Orphan tab — accept any slug that came in via the form (we just
            // need to find it in the DB to update it).
            var orphans = await BuildOrphanFieldsAsync(ct);
            fieldsToUpdate = orphans;
        }
        else if (!string.IsNullOrEmpty(pageId))
        {
            var page = ContentPages.FirstOrDefault(p => p.Id == pageId);
            fieldsToUpdate = page?.Fields ?? Enumerable.Empty<ContentField>();
        }
        else
        {
            fieldsToUpdate = ContentPages.SelectMany(p => p.Fields);
        }

        foreach (var field in fieldsToUpdate)
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

        if (updated > 0)
        {
            await _db.SaveChangesAsync(ct);
            TempData["ToastOk"] = $"{updated} field(s) saved.";
        }
        else
        {
            TempData["ToastOk"] = "No changes detected.";
        }

        return RedirectToPage(null, null, pageId);
    }

    public async Task<IActionResult> OnPostBrandingAsync(CancellationToken ct)
    {
        ViewData["AdminSection"] = "website";
        var form = Request.Form;
        int updated = 0;

        var allSettings = await _db.SiteSettings.ToListAsync(ct);
        var byKey = allSettings.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase);

        foreach (var (key, _, _) in BrandingFields)
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

        return RedirectToPage(null, null, "branding");
    }

    private async Task LoadDataAsync(CancellationToken ct)
    {
        var blocks = await _db.ContentBlocks.AsNoTracking().ToListAsync(ct);
        BlockBodies = blocks.ToDictionary(b => b.Slug, b => b.Body, StringComparer.OrdinalIgnoreCase);

        var settings = await _db.SiteSettings.AsNoTracking().ToListAsync(ct);
        Settings = settings.ToDictionary(s => s.Key, s => s, StringComparer.OrdinalIgnoreCase);

        OrphanFields = await BuildOrphanFieldsAsync(ct);
    }

    private async Task<List<ContentField>> BuildOrphanFieldsAsync(CancellationToken ct)
    {
        var wired = ContentPages.SelectMany(p => p.Fields)
            .Select(f => f.Slug)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allSlugs = await _db.ContentBlocks
            .AsNoTracking()
            .Select(b => b.Slug)
            .ToListAsync(ct);

        return allSlugs
            .Where(s => !wired.Contains(s))
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Select(s => new ContentField(s, PrettifyLabel(s), IsTextarea: true,
                Hint: "Not yet wired to a page editor — slug-only edit."))
            .ToList();
    }

    /// <summary>Turn a slug like "page.about.story.body" into "Page · About · Story · Body".</summary>
    private static string PrettifyLabel(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return slug;
        var parts = slug.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var pretty = parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]);
        return string.Join(" · ", pretty);
    }
}
