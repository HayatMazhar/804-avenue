using System.Linq;
using System.Threading;
using Avenue804.Web.Configuration;
using Avenue804.Web.Domain;
using Avenue804.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Avenue804.Web.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";

    public static async Task EnsureSeededAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();
        var siteOptions = services.GetRequiredService<IOptions<SiteOptions>>().Value;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData");
        var ct = CancellationToken.None;

        await db.Database.MigrateAsync();

        await EnsureSiteSettingsAsync(db, siteOptions, logger, ct);
        await EnsureInquiryTopicLookupsAsync(db, logger, ct);
        await EnsurePropertyInquiryLookupsAsync(db, logger, ct);
        await EnsurePropertyInquiryBudgetAndShowroomAsync(db, logger, ct);
        await EnsureHomeContentBlocksAsync(db, logger, ct);
        await EnsureBodyContentBlocksAsync(db, logger, ct);
        await EnsureUaeAreasAsync(db, logger, ct);

        await TrySeedDemoCatalogAsync(db, config, logger, ct);

        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole(AdminRole));

        var adminEmail = config["Seed:AdminEmail"] ?? "admin@804avenue.com";
        var adminPassword = config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogWarning(
                "Seed:AdminPassword is not set. Skipping admin user creation. Set it in appsettings.Development.json or environment variables for first-time setup.");
            return;
        }

        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing != null)
            return;

        var user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            DisplayName = "Administrator"
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (!result.Succeeded)
        {
            logger.LogError("Failed to create admin user: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await userManager.AddToRoleAsync(user, AdminRole);
        logger.LogInformation("Admin user {Email} created with role {Role}.", adminEmail, AdminRole);
    }

    private static async Task EnsureSiteSettingsAsync(
        ApplicationDbContext db,
        SiteOptions options,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var existing = await db.SiteSettings.Select(s => s.Key).ToListAsync(cancellationToken);
        var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTimeOffset.UtcNow;
        foreach (var row in SiteBrandingService.BuildDefaultRowsFromOptions(options))
        {
            if (set.Contains(row.Key))
                continue;
            row.UpdatedAt = now;
            db.SiteSettings.Add(row);
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded missing site settings from appsettings Site section.");
        }
    }

    private static async Task EnsureInquiryTopicLookupsAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var cat = await db.LookupCategories.FirstOrDefaultAsync(
            c => c.Code == LookupCategories.InquiryTopic,
            cancellationToken);
        if (cat != null)
            return;

        cat = new LookupCategory
        {
            Code = LookupCategories.InquiryTopic,
            Name = "Inquiry topic",
            SortOrder = 0,
            Values =
            [
                new LookupValue { Code = "general", DisplayName = "General inquiry", SortOrder = 0, IsActive = true },
                new LookupValue { Code = "properties", DisplayName = "Properties", SortOrder = 10, IsActive = true },
                new LookupValue { Code = "contracting", DisplayName = "Contracting", SortOrder = 20, IsActive = true },
                new LookupValue { Code = "maintenance", DisplayName = "Maintenance", SortOrder = 30, IsActive = true },
                new LookupValue { Code = "facility", DisplayName = "Facility management", SortOrder = 40, IsActive = true }
            ]
        };
        db.LookupCategories.Add(cat);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded inquiry topic lookup category and values.");
    }

    private static async Task EnsurePropertyInquiryLookupsAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.LookupCategories.AnyAsync(c => c.Code == LookupCategories.PropertyInquiryWantTo, cancellationToken))
            return;

        db.LookupCategories.AddRange(
            new LookupCategory
            {
                Code = LookupCategories.PropertyInquiryIAm,
                Name = "Property inquiry — I am",
                SortOrder = 11,
                Values =
                [
                    new LookupValue { Code = "individual", DisplayName = "Individual", SortOrder = 0, IsActive = true },
                    new LookupValue { Code = "company", DisplayName = "Company", SortOrder = 10, IsActive = true },
                    new LookupValue { Code = "agent", DisplayName = "Agent", SortOrder = 20, IsActive = true },
                    new LookupValue { Code = "freelancer", DisplayName = "Free Lancer", SortOrder = 30, IsActive = true }
                ]
            },
            new LookupCategory
            {
                Code = LookupCategories.PropertyInquiryWantTo,
                Name = "Property inquiry — I want to",
                SortOrder = 12,
                Values =
                [
                    new LookupValue { Code = "rent", DisplayName = "Rent", SortOrder = 0, IsActive = true },
                    new LookupValue { Code = "buy", DisplayName = "Buy", SortOrder = 10, IsActive = true },
                    new LookupValue { Code = "sell", DisplayName = "Sell", SortOrder = 20, IsActive = true }
                ]
            },
            new LookupCategory
            {
                Code = LookupCategories.PropertyInquiryPropertyType,
                Name = "Property inquiry — property type",
                SortOrder = 13,
                Values =
                [
                    new LookupValue { Code = "residential", DisplayName = "Residential", SortOrder = 0, IsActive = true },
                    new LookupValue { Code = "commercial", DisplayName = "Commercial", SortOrder = 10, IsActive = true },
                    new LookupValue { Code = "industrial", DisplayName = "Industrial", SortOrder = 20, IsActive = true }
                ]
            },
            new LookupCategory
            {
                Code = LookupCategories.PropertyInquiryPropertyDetail,
                Name = "Property inquiry — property details",
                SortOrder = 14,
                Values =
                [
                    new LookupValue { Code = "villa-compound", DisplayName = "Villa Compound", SortOrder = 0, IsActive = true, Metadata = """{"forTypes":["residential"]}""" },
                    new LookupValue { Code = "apartment", DisplayName = "Apartment", SortOrder = 10, IsActive = true, Metadata = """{"forTypes":["residential"]}""" },
                    new LookupValue { Code = "penthouse", DisplayName = "Penthouse", SortOrder = 20, IsActive = true, Metadata = """{"forTypes":["residential"]}""" },
                    new LookupValue { Code = "townhouse", DisplayName = "Townhouse", SortOrder = 30, IsActive = true, Metadata = """{"forTypes":["residential"]}""" },
                    new LookupValue { Code = "office", DisplayName = "Office Space", SortOrder = 40, IsActive = true, Metadata = """{"forTypes":["commercial"]}""" },
                    new LookupValue { Code = "retail", DisplayName = "Retail Unit", SortOrder = 50, IsActive = true, Metadata = """{"forTypes":["commercial"]}""" },
                    new LookupValue { Code = "warehouse", DisplayName = "Warehouse", SortOrder = 60, IsActive = true, Metadata = """{"forTypes":["industrial"]}""" },
                    new LookupValue { Code = "land", DisplayName = "Land / Plot", SortOrder = 70, IsActive = true, Metadata = """{"forTypes":["commercial","industrial"]}""" }
                ]
            },
            new LookupCategory
            {
                Code = LookupCategories.PropertyInquiryLocation,
                Name = "Property inquiry — location",
                SortOrder = 15,
                Values =
                [
                    new LookupValue { Code = "abu-dhabi", DisplayName = "Abu Dhabi", SortOrder = 0, IsActive = true },
                    new LookupValue { Code = "dubai", DisplayName = "Dubai", SortOrder = 10, IsActive = true },
                    new LookupValue { Code = "sharjah", DisplayName = "Sharjah", SortOrder = 20, IsActive = true },
                    new LookupValue { Code = "al-ain", DisplayName = "Al Ain", SortOrder = 30, IsActive = true },
                    new LookupValue { Code = "northern-emirates", DisplayName = "Northern Emirates", SortOrder = 40, IsActive = true },
                    new LookupValue { Code = "other-uae", DisplayName = "Other UAE", SortOrder = 50, IsActive = true }
                ]
            });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded property listing inquiry lookup categories and values.");
    }

    private static async Task EnsurePropertyInquiryBudgetAndShowroomAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (!await db.LookupCategories.AnyAsync(c => c.Code == LookupCategories.PropertyInquiryBudget, cancellationToken))
        {
            db.LookupCategories.Add(new LookupCategory
            {
                Code = LookupCategories.PropertyInquiryBudget,
                Name = "Property inquiry — budget",
                SortOrder = 16,
                Values =
                [
                    new LookupValue { Code = "under-500k", DisplayName = "Under AED 500,000", SortOrder = 0, IsActive = true },
                    new LookupValue { Code = "500k-1m", DisplayName = "AED 500,000 – 1,000,000", SortOrder = 10, IsActive = true },
                    new LookupValue { Code = "1m-2m", DisplayName = "AED 1,000,000 – 2,000,000", SortOrder = 20, IsActive = true },
                    new LookupValue { Code = "2m-5m", DisplayName = "AED 2,000,000 – 5,000,000", SortOrder = 30, IsActive = true },
                    new LookupValue { Code = "5m-10m", DisplayName = "AED 5,000,000 – 10,000,000", SortOrder = 40, IsActive = true },
                    new LookupValue { Code = "over-10m", DisplayName = "Over AED 10,000,000", SortOrder = 50, IsActive = true },
                    new LookupValue { Code = "discuss", DisplayName = "Prefer to discuss", SortOrder = 60, IsActive = true }
                ]
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded property inquiry budget lookup category.");
        }

        var detailCat = await db.LookupCategories
            .Include(c => c.Values)
            .FirstOrDefaultAsync(c => c.Code == LookupCategories.PropertyInquiryPropertyDetail, cancellationToken);
        if (detailCat != null && detailCat.Values.All(v => v.Code != "showroom"))
        {
            detailCat.Values.Add(new LookupValue
            {
                Code = "showroom",
                DisplayName = "Showroom",
                SortOrder = 45,
                IsActive = true,
                Metadata = """{"forTypes":["commercial"]}"""
            });
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Added Showroom to property inquiry property details.");
        }
    }

    private static async Task EnsureHomeContentBlocksAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var specs = new (string Slug, string? Title, string Body)[]
        {
            (ContentBlockSlugs.HomeHeroEyebrow, null, "Abu Dhabi's Trusted Property, Construction &amp; Maintenance Partner"),
            (ContentBlockSlugs.HomeHeroSubtitle, null,
                "From premium real estate services to complete construction, fit-out, and maintenance solutions in Abu Dhabi — 804 Avenue is your trusted all-in-one partner."),
            (ContentBlockSlugs.AboutStripLead, null,
                "804 Avenue Properties and Contracting is a dynamic company offering professional services in the fields of real estate, construction, and property maintenance across the UAE. Headquartered in Abu Dhabi, we combine deep market knowledge with skilled craftsmanship to deliver exceptional results for every client."),
            (ContentBlockSlugs.PageAboutHeroTitle, null, "About <em>804 Avenue</em>"),
            (ContentBlockSlugs.PageAboutHeroSubtitle, null,
                "Building trust, delivering excellence, and shaping the future of real estate and construction across the UAE."),
            (ContentBlockSlugs.PageContractingHeroTitle, null, "Crafting <em>Spaces</em> with Quality and Precision"),
            (ContentBlockSlugs.PageContractingHeroSubtitle, null,
                "Our contracting division delivers reliable construction and fit-out services with a focus on quality craftsmanship, professional project management, and on-time delivery across the UAE."),
            (ContentBlockSlugs.PageMaintenanceHeroTitle, null, "Preserving <em>Value</em> Through Professional Care"),
            (ContentBlockSlugs.PageMaintenanceHeroSubtitle, null,
                "Our certified maintenance team ensures your property stays in peak condition with responsive, reliable care — from scheduled inspections to emergency repairs across Abu Dhabi and the UAE."),
            (ContentBlockSlugs.PageFacilityHeroTitle, null, "Integrated <em>Facility</em> Management"),
            (ContentBlockSlugs.PageFacilityHeroSubtitle, null,
                "End-to-end facility operations for offices, retail, and residential towers — combining maintenance, soft services, security coordination, and vendor management under one accountable partner across Abu Dhabi and the UAE.")
        };

        foreach (var (slug, title, body) in specs)
        {
            if (await db.ContentBlocks.AnyAsync(b => b.Slug == slug, cancellationToken))
                continue;
            db.ContentBlocks.Add(new ContentBlock
            {
                Slug = slug,
                Title = title,
                Body = body,
                IsPublished = true,
                UpdatedAt = now
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded default content blocks (home, about strip, service page heroes).");
        }
    }

    private static async Task EnsureBodyContentBlocksAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var specs = new (string Slug, string Body)[]
        {
            // ── Home page ───────────────────────────────────────────────────────
            (ContentBlockSlugs.HomeHeroHeadline, "Build.<br><span class=\"gradient\">Buy.</span><br>Maintain."),
            (ContentBlockSlugs.HomeAboutTag, "Who We Are"),
            (ContentBlockSlugs.HomeAboutTitle, "Your Trusted <em>Partner</em> in Property &amp; Construction"),
            (ContentBlockSlugs.HomeAboutCards, """
<div class="about-card fade-up"><div class="about-card-icon"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg></div><div><h4>Our Vision</h4><p>To be the most trusted and innovative real estate and contracting company in the UAE.</p></div></div>
<div class="about-card fade-up"><div class="about-card-icon"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="12" cy="12" r="3"/><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg></div><div><h4>Our Mission</h4><p>To deliver exceptional quality in every property transaction, construction project, and maintenance service.</p></div></div>
<div class="about-card fade-up"><div class="about-card-icon"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M20 6L9 17l-5-5"/></svg></div><div><h4>Our Values</h4><p>Integrity, professionalism, quality, innovation, and an unwavering focus on client satisfaction.</p></div></div>
"""),
            (ContentBlockSlugs.HomeStatsItems, """
<div class="fade-up"><div class="stat-num" data-count="500" data-suffix="+">0</div><div class="stat-label">Properties Listed</div></div>
<div class="fade-up"><div class="stat-num" data-count="120" data-suffix="+">0</div><div class="stat-label">Projects Delivered</div></div>
<div class="fade-up"><div class="stat-num" data-count="15" data-suffix="+">0</div><div class="stat-label">Years Experience</div></div>
<div class="fade-up"><div class="stat-num" data-count="98" data-suffix="%">0</div><div class="stat-label">Client Satisfaction</div></div>
"""),
            (ContentBlockSlugs.HomeReTag, "Real Estate"),
            (ContentBlockSlugs.HomeReTitle, "Your Trusted <em>Property</em> Partner"),
            (ContentBlockSlugs.HomeContractingTag, "Contracting"),
            (ContentBlockSlugs.HomeContractingTitle, "Crafting <em>Spaces</em> with Quality and Precision"),
            (ContentBlockSlugs.HomeContractingLead, "Our contracting division provides reliable construction and fit-out services with a focus on quality craftsmanship and professional project management."),
            (ContentBlockSlugs.HomeMaintenanceTag, "Maintenance"),
            (ContentBlockSlugs.HomeMaintenanceTitle, "Preserving <em>Value</em> Through Professional Care"),
            (ContentBlockSlugs.HomeMaintenanceLead, "Our certified maintenance team ensures your property stays in peak condition with responsive, reliable care."),
            (ContentBlockSlugs.HomeWhyTag, "Why 804 Avenue"),
            (ContentBlockSlugs.HomeWhyTitle, "One Partner. <em>Every</em> Solution."),
            (ContentBlockSlugs.HomeWhyItems, """
<div class="why-item fade-up"><div class="why-num">01</div><h4>All-in-One Provider</h4><p>Real estate, contracting, and maintenance under one roof.</p></div>
<div class="why-item fade-up"><div class="why-num">02</div><h4>Licensed &amp; Certified</h4><p>Fully licensed engineers, technicians, and consultants.</p></div>
<div class="why-item fade-up"><div class="why-num">03</div><h4>On-Time Delivery</h4><p>Structured project management at every phase.</p></div>
<div class="why-item fade-up"><div class="why-num">04</div><h4>Transparent Pricing</h4><p>No hidden fees — detailed cost breakdowns always.</p></div>
<div class="why-item fade-up"><div class="why-num">05</div><h4>Client-First</h4><p>Responsive support before, during, and after.</p></div>
"""),
            (ContentBlockSlugs.HomeTestiTag, "Client Stories"),
            (ContentBlockSlugs.HomeTestiTitle, "What Our <em>Clients</em> Say"),
            (ContentBlockSlugs.HomeTestiItems, """
<div class="testi-card fade-up"><div class="testi-quote">&ldquo;</div><p class="testi-text">804 Avenue helped us find the perfect apartment in Al Reem Island and handled the complete interior fit-out. One company for everything — seamless.</p><div class="testi-author">Fatima Al Mazrouei</div><div class="testi-role">Homeowner — Al Reem Island</div></div>
<div class="testi-card fade-up"><div class="testi-quote">&ldquo;</div><p class="testi-text">Their contracting team delivered our office renovation ahead of schedule. The quality of gypsum and painting work was exceptional. Highly recommended.</p><div class="testi-author">James Mitchell</div><div class="testi-role">CEO — Mitchell Group</div></div>
<div class="testi-card fade-up"><div class="testi-quote">&ldquo;</div><p class="testi-text">We've used 804 Avenue for our annual maintenance contract for two years. Their preventive approach has saved us from major issues. Professional and reliable.</p><div class="testi-author">Ahmed Al Dhaheri</div><div class="testi-role">Building Manager — Abu Dhabi</div></div>
"""),
            (ContentBlockSlugs.HomeCtaTitle, "Ready to Build, Buy or Maintain?"),

            // ── About page ──────────────────────────────────────────────────────
            (ContentBlockSlugs.AboutStoryTag, "Our Story"),
            (ContentBlockSlugs.AboutStoryTitle, "A Legacy of <em>Excellence</em>"),
            (ContentBlockSlugs.AboutStoryBody, """
<p>804 Avenue Properties and Contracting was founded with a clear vision: to become Abu Dhabi's most trusted and integrated partner for real estate, construction, and property maintenance services.</p>
<p>Headquartered in the heart of Abu Dhabi at Al Ghaith Tower, we combine deep market knowledge with skilled craftsmanship to deliver exceptional results for every client — from individual homeowners to large-scale commercial enterprises.</p>
<p>Over the years, we have built a reputation for professionalism, quality, and reliability. Our team of licensed engineers, certified technicians, and experienced consultants work together seamlessly to provide end-to-end solutions across the property lifecycle.</p>
<p>Whether you're buying your dream home, building a new office, or maintaining a residential tower — 804 Avenue is your single point of contact for everything property.</p>
"""),
            (ContentBlockSlugs.AboutVmTag, "Our Purpose"),
            (ContentBlockSlugs.AboutVisionBody, "To be the most trusted and innovative real estate and contracting company in the UAE — recognized for excellence in every property transaction, construction project, and maintenance service we deliver."),
            (ContentBlockSlugs.AboutMissionBody, "To deliver exceptional quality in every property transaction, construction project, and maintenance service — by combining deep expertise, premium materials, and an unwavering commitment to client satisfaction."),
            (ContentBlockSlugs.AboutValuesTag, "Our Principles"),
            (ContentBlockSlugs.AboutValuesItems, """
<div class="value-item fade-up"><div class="value-num">01</div><div><h4>Integrity</h4><p>We operate with complete transparency and honesty in every interaction — building trust through actions, not just words.</p></div></div>
<div class="value-item fade-up"><div class="value-num">02</div><div><h4>Professionalism</h4><p>Every team member upholds the highest standards of conduct, communication, and workmanship in all that we do.</p></div></div>
<div class="value-item fade-up"><div class="value-num">03</div><div><h4>Quality</h4><p>We never compromise on materials, processes, or outcomes. Quality is embedded in every phase of our work.</p></div></div>
<div class="value-item fade-up"><div class="value-num">04</div><div><h4>Innovation</h4><p>We embrace modern methods, technologies, and design thinking to deliver smarter, faster, and better solutions.</p></div></div>
<div class="value-item fade-up"><div class="value-num">05</div><div><h4>Client Satisfaction</h4><p>Our clients are at the center of everything we do. We measure our success by the satisfaction and loyalty of those we serve.</p></div></div>
"""),
            (ContentBlockSlugs.AboutExpertiseTag, "Capabilities"),
            (ContentBlockSlugs.AboutExpertiseLead, "Across three integrated divisions, we bring deep domain knowledge and hands-on experience to every engagement."),
            (ContentBlockSlugs.AboutExpertiseItems, """
<div class="expertise-item fade-up"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg><span><strong style="color:var(--text-primary)">Real Estate Brokerage &amp; Advisory</strong> — Buying, selling, leasing, and investment advisory for residential, commercial, and off-plan properties across Abu Dhabi and Dubai.</span></div>
<div class="expertise-item fade-up"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg><span><strong style="color:var(--text-primary)">Construction &amp; Contracting</strong> — Civil works, gypsum, glass &amp; aluminum, painting, flooring, interior fit-out, renovation, and signage — from concept to handover.</span></div>
<div class="expertise-item fade-up"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg><span><strong style="color:var(--text-primary)">Property Maintenance</strong> — General building, civil, MEP, preventive, and corrective maintenance services with 24/7 emergency support.</span></div>
<div class="expertise-item fade-up"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg><span><strong style="color:var(--text-primary)">Project Management</strong> — End-to-end project coordination with dedicated site engineers, safety officers, and quality controllers on every project.</span></div>
<div class="expertise-item fade-up"><svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg><span><strong style="color:var(--text-primary)">Design &amp; Consultation</strong> — Space planning, material selection, MEP coordination, and authority approval support for all project types.</span></div>
"""),
            (ContentBlockSlugs.AboutTeamLead, "The people behind every successful project, transaction, and client relationship at 804 Avenue."),
            (ContentBlockSlugs.AboutTeamItems, """
<div class="team-card fade-up"><div class="team-avatar"><span class="team-initials">AA</span></div><h4>Ahmed Al Mansoori</h4><p>Managing Director</p><span class="team-desc">Leads the company's strategic direction with over 18 years of experience in UAE real estate and construction.</span></div>
<div class="team-card fade-up"><div class="team-avatar"><span class="team-initials">MZ</span></div><h4>Mohammed Al Zaabi</h4><p>Head of Contracting</p><span class="team-desc">Oversees all construction and fit-out projects, ensuring quality delivery and client satisfaction on every site.</span></div>
<div class="team-card fade-up"><div class="team-avatar"><span class="team-initials">SH</span></div><h4>Sara Al Hashimi</h4><p>Real Estate Director</p><span class="team-desc">Manages the property division with deep knowledge of Abu Dhabi and Dubai's residential and commercial markets.</span></div>
<div class="team-card fade-up"><div class="team-avatar"><span class="team-initials">KN</span></div><h4>Khalid Al Nuaimi</h4><p>Maintenance Manager</p><span class="team-desc">Leads the maintenance team with a focus on preventive care, rapid response, and long-term asset protection.</span></div>
"""),
            (ContentBlockSlugs.AboutWcuTag, "Why 804 Avenue"),
            (ContentBlockSlugs.AboutWcuItems, """
<div class="wcu-item fade-up"><div class="wcu-num">01</div><h4>All-in-One Provider</h4><p>Real estate, contracting, and maintenance — all under one roof for seamless service.</p></div>
<div class="wcu-item fade-up"><div class="wcu-num">02</div><h4>Licensed &amp; Certified</h4><p>Fully licensed engineers, certified technicians, and accredited consultants on every project.</p></div>
<div class="wcu-item fade-up"><div class="wcu-num">03</div><h4>On-Time Delivery</h4><p>Structured project management and milestone tracking ensure we deliver on schedule.</p></div>
<div class="wcu-item fade-up"><div class="wcu-num">04</div><h4>Transparent Pricing</h4><p>No hidden fees — detailed cost breakdowns and clear payment milestones from day one.</p></div>
<div class="wcu-item fade-up"><div class="wcu-num">05</div><h4>Client-First Approach</h4><p>Responsive support before, during, and after every project — your satisfaction is our priority.</p></div>
"""),

            // ── Contracting page ────────────────────────────────────────────────
            (ContentBlockSlugs.ContractingSvc1Title, "Civil &amp; General <em>Contracting</em>"),
            (ContentBlockSlugs.ContractingSvc2Title, "Gypsum &amp; <em>Ceiling</em> Works"),
            (ContentBlockSlugs.ContractingSvc3Title, "Glass &amp; <em>Aluminum</em> Works"),
            (ContentBlockSlugs.ContractingSvc4Title, "Painting &amp; <em>Finishing</em>"),
            (ContentBlockSlugs.ContractingSvc5Title, "Premium <em>Flooring</em> Solutions"),
            (ContentBlockSlugs.ContractingSvc6Title, "Interior Fit-Out &amp; <em>Renovation</em>"),
            (ContentBlockSlugs.ContractingSvc7Title, "Internal &amp; External <em>Signage</em>"),
            (ContentBlockSlugs.ContractingProcessTitle, "From Concept to <em>Completion</em>"),
            (ContentBlockSlugs.ContractingProcessLead, "A streamlined, transparent approach that keeps your project on track from day one to handover."),
            (ContentBlockSlugs.ContractingCtaTitle, "Ready to Start Your <em>Project</em>?"),
            (ContentBlockSlugs.ContractingCtaBody, "Let's discuss your contracting needs. From initial consultation to final handover, we're with you every step of the way."),

            // ── Maintenance page ────────────────────────────────────────────────
            (ContentBlockSlugs.MaintenanceSvc1Title, "General <em>Building</em> Maintenance"),
            (ContentBlockSlugs.MaintenanceSvc2Title, "Civil <em>Maintenance</em>"),
            (ContentBlockSlugs.MaintenanceSvc3Title, "<em>MEP</em> Maintenance"),
            (ContentBlockSlugs.MaintenanceSvc4Title, "Preventive <em>Maintenance</em>"),
            (ContentBlockSlugs.MaintenanceSvc5Title, "Corrective <em>Maintenance</em>"),
            (ContentBlockSlugs.MaintenanceContractTitle, "Get a Tailored <em>Maintenance Contract</em>"),
            (ContentBlockSlugs.MaintenanceContractBody, "Our Annual Maintenance Contracts (AMC) are customized to your property's needs — covering all systems, all schedules, with dedicated account management and priority response."),
            (ContentBlockSlugs.MaintenanceCtaTitle, "Keep Your Property in <em>Peak</em> Condition"),
            (ContentBlockSlugs.MaintenanceCtaBody, "From scheduled inspections to emergency repairs, our maintenance team is here for you — 24/7, 365 days a year."),

            // ── Facility Management page ────────────────────────────────────────
            (ContentBlockSlugs.FacilitySvc1Title, "Building <em>Operations</em> &amp; Compliance"),
            (ContentBlockSlugs.FacilitySvc2Title, "Soft <em>Services</em> &amp; Guest Experience"),
            (ContentBlockSlugs.FacilityWhyTag, "Why FM with 804"),
            (ContentBlockSlugs.FacilityWhyTitle, "One accountable <em>partner</em>"),
            (ContentBlockSlugs.FacilityCtaTitle, "Discuss your <em>facility</em> scope"),
            (ContentBlockSlugs.FacilityCtaBody, "Share your asset type, size, and service hours — we will propose a tailored FM model and transition plan.")
        };

        foreach (var (slug, body) in specs)
        {
            if (await db.ContentBlocks.AnyAsync(b => b.Slug == slug, cancellationToken))
                continue;
            db.ContentBlocks.Add(new ContentBlock
            {
                Slug = slug,
                Body = body,
                IsPublished = true,
                UpdatedAt = now
            });
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded {Count} body content blocks.", specs.Length);
        }
    }

    private static async Task TrySeedDemoCatalogAsync(
        ApplicationDbContext db,
        IConfiguration config,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(config["Seed:DemoCatalog"], "true", StringComparison.OrdinalIgnoreCase))
            return;

        if (await db.PropertyListings.AnyAsync(cancellationToken))
            return;

        var now = DateTimeOffset.UtcNow;
        db.PropertyListings.Add(new PropertyListing
        {
            Title = "Demo 2BR apartment (Al Reem)",
            Slug = "demo-2br-al-reem",
            Price = 1_200_000,
            Currency = "AED",
            OfferType = ListingOfferType.Sale,
            Location = "Al Reem Island, Abu Dhabi",
            Description = "Sample published listing for local development. Replace or delete in admin.",
            Beds = 2,
            Baths = 2,
            AreaSqft = 1250,
            MainImageUrl = "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=1200&q=80",
            IsPublished = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        db.PortfolioProjects.Add(new PortfolioProject
        {
            Title = "Demo fit-out project",
            Slug = "demo-fit-out",
            Summary = "Sample portfolio entry for local development.",
            Description = "Replace this text with a real project narrative in admin.",
            CoverImageUrl = "https://images.unsplash.com/photo-1618221195710-dd6b41faaea6?w=1200&q=80",
            IsPublished = true,
            CreatedAt = now
        });

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Demo catalog seeded (property + project).");
    }

    private static async Task EnsureUaeAreasAsync(ApplicationDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.UaeAreas.AnyAsync(ct))
            return;

        var areas = new List<Domain.UaeArea>();
        int order = 1000;

        void Add(string name, string emirate, string? city, Domain.AreaType type, int sort = 0)
            => areas.Add(new Domain.UaeArea { Name = name, Emirate = emirate, City = city, Type = type, SortOrder = sort > 0 ? sort : order-- });

        // ── ABU DHABI CITY ─────────────────────────────────────────────
        Add("Al Reem Island",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Island,     990);
        Add("Saadiyat Island",        "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Island,     980);
        Add("Yas Island",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Island,     975);
        Add("Al Raha Beach",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  970);
        Add("Al Maryah Island",       "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Island,     965);
        Add("Khalifa City",           "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  960);
        Add("Khalifa City A",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  958);
        Add("Khalifa City B",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  956);
        Add("Mohammed Bin Zayed City","Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  955);
        Add("Al Mushrif",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   950);
        Add("Corniche Road",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   948);
        Add("Hamdan Street",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   946);
        Add("Airport Road",           "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   944);
        Add("Tourist Club Area",      "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   940);
        Add("Khalidiyah",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   938);
        Add("Al Bateen",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   936);
        Add("Al Markaziyah",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   934);
        Add("Al Karama",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   932);
        Add("Al Wahda",               "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   930);
        Add("Al Manaseer",            "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   928);
        Add("Al Rowdah",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   926);
        Add("Al Muroor",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   924);
        Add("Al Zaab",                "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   922);
        Add("Al Mina",                "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   920);
        Add("Al Qurm",                "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   918);
        Add("Al Khubeirah",           "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   916);
        Add("Electra Street",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   914);
        Add("Zayed Port",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   910);
        Add("Al Shamkha",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  908);
        Add("Masdar City",            "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  906);
        Add("Al Reef",                "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  904);
        Add("Al Reef Downtown",       "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  902);
        Add("Al Reef Villas",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  900);
        Add("Hydra Village",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  898);
        Add("Al Falah",               "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  896);
        Add("Al Ghadeer",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  894);
        Add("Zayed City",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  892);
        Add("Al Shawamekh",           "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  890);
        Add("Al Rahba",               "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  888);
        Add("Baniyas",                "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   886);
        Add("Al Musaffah",            "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Industrial, 884);
        Add("Mussafah",               "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Industrial, 882);
        Add("ICAD",                   "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Industrial, 880);
        Add("Madinat Zayed",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   878);
        Add("Al Raha Gardens",        "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Community,  876);
        Add("Al Jubail",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   874);
        Add("Al Khaznah",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   872);
        // Yas Island developments
        Add("Yas Acres",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 870);
        Add("Yas Park Views",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 868);
        Add("Yas Bay",                "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 866);
        Add("West Yas",               "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 864);
        Add("Ansam",                  "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 862);
        Add("Reflection",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 860);
        Add("Noya",                   "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 858);
        Add("Noya Viva",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 856);
        Add("Water's Edge",           "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 854);
        Add("Mamsha Al Saadiyat",     "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 852);
        Add("Louvre Abu Dhabi",       "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 850);
        Add("The Domain",             "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 848);
        Add("Nation Towers",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Building,   846);
        Add("Etihad Towers",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Building,   844);
        Add("Gate Towers",            "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Building,   842);
        Add("Sun Tower",              "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Building,   840);
        Add("City of Lights",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 838);
        Add("Shams Abu Dhabi",        "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 836);
        Add("Marina Square",          "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Development, 834);
        Add("Al Ghaith Tower",        "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Building,   832);
        Add("Mangrove Place",         "Abu Dhabi", "Abu Dhabi", Domain.AreaType.Building,   830);
        Add("ADGM",                   "Abu Dhabi", "Abu Dhabi", Domain.AreaType.FreeZone,   828);
        Add("ADNEC",                  "Abu Dhabi", "Abu Dhabi", Domain.AreaType.District,   826);
        Add("Abu Dhabi Global Market","Abu Dhabi", "Abu Dhabi", Domain.AreaType.FreeZone,   824);

        // ── AL AIN ──────────────────────────────────────────────────────
        Add("Al Ain City",            "Abu Dhabi", "Al Ain",   Domain.AreaType.City,        820);
        Add("Al Jimi",                "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    818);
        Add("Al Muwaiji",             "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    816);
        Add("Al Towayya",             "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    814);
        Add("Al Hili",                "Abu Dhabi", "Al Ain",   Domain.AreaType.Community,   812);
        Add("Zakher",                 "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    810);
        Add("Al Yahar",               "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    808);
        Add("Mezyad",                 "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    806);
        Add("Al Khabisi",             "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    804);
        Add("Al Mutaredh",            "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    802);
        Add("Al Markhaniyah",         "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    800);
        Add("Al Qattara",             "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    798);
        Add("Al Ain Oasis",           "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    796);
        Add("Al Jimi Mall Area",      "Abu Dhabi", "Al Ain",   Domain.AreaType.District,    794);
        Add("Al Ain Industrial",      "Abu Dhabi", "Al Ain",   Domain.AreaType.Industrial,  792);

        // ── AL DHAFRA / WESTERN REGION ────────────────────────────────
        Add("Ruwais",                 "Abu Dhabi", "Al Dhafra", Domain.AreaType.City,       790);
        Add("Madinat Zayed (Liwa)",   "Abu Dhabi", "Al Dhafra", Domain.AreaType.City,       788);
        Add("Ghayathi",               "Abu Dhabi", "Al Dhafra", Domain.AreaType.City,       786);
        Add("Liwa",                   "Abu Dhabi", "Al Dhafra", Domain.AreaType.District,   784);
        Add("Mirfa",                  "Abu Dhabi", "Al Dhafra", Domain.AreaType.City,       782);
        Add("Sila",                   "Abu Dhabi", "Al Dhafra", Domain.AreaType.City,       780);

        // ── DUBAI ────────────────────────────────────────────────────────
        Add("Downtown Dubai",         "Dubai", "Dubai", Domain.AreaType.Community,  990);
        Add("Dubai Marina",           "Dubai", "Dubai", Domain.AreaType.Community,  988);
        Add("Jumeirah Beach Residence","Dubai","Dubai", Domain.AreaType.Community,  986);
        Add("JBR",                    "Dubai", "Dubai", Domain.AreaType.Community,  984);
        Add("Business Bay",           "Dubai", "Dubai", Domain.AreaType.Community,  982);
        Add("DIFC",                   "Dubai", "Dubai", Domain.AreaType.FreeZone,   980);
        Add("Palm Jumeirah",          "Dubai", "Dubai", Domain.AreaType.Island,     978);
        Add("Dubai Hills Estate",     "Dubai", "Dubai", Domain.AreaType.Community,  976);
        Add("Jumeirah Village Circle","Dubai", "Dubai", Domain.AreaType.Community,  974);
        Add("JVC",                    "Dubai", "Dubai", Domain.AreaType.Community,  973);
        Add("Jumeirah Village Triangle","Dubai","Dubai", Domain.AreaType.Community, 972);
        Add("JVT",                    "Dubai", "Dubai", Domain.AreaType.Community,  971);
        Add("Jumeirah Lakes Towers",  "Dubai", "Dubai", Domain.AreaType.Community,  970);
        Add("JLT",                    "Dubai", "Dubai", Domain.AreaType.Community,  969);
        Add("City Walk",              "Dubai", "Dubai", Domain.AreaType.Development, 968);
        Add("Bluewaters Island",      "Dubai", "Dubai", Domain.AreaType.Island,     966);
        Add("Al Barsha",              "Dubai", "Dubai", Domain.AreaType.District,   964);
        Add("Al Barsha 1",            "Dubai", "Dubai", Domain.AreaType.District,   963);
        Add("Al Barsha 2",            "Dubai", "Dubai", Domain.AreaType.District,   962);
        Add("Al Barsha 3",            "Dubai", "Dubai", Domain.AreaType.District,   961);
        Add("Al Barsha South",        "Dubai", "Dubai", Domain.AreaType.District,   960);
        Add("Jumeirah",               "Dubai", "Dubai", Domain.AreaType.District,   958);
        Add("Jumeirah 1",             "Dubai", "Dubai", Domain.AreaType.District,   957);
        Add("Jumeirah 2",             "Dubai", "Dubai", Domain.AreaType.District,   956);
        Add("Jumeirah 3",             "Dubai", "Dubai", Domain.AreaType.District,   955);
        Add("Umm Suqeim",             "Dubai", "Dubai", Domain.AreaType.District,   954);
        Add("Umm Suqeim 1",           "Dubai", "Dubai", Domain.AreaType.District,   953);
        Add("Umm Suqeim 2",           "Dubai", "Dubai", Domain.AreaType.District,   952);
        Add("Umm Suqeim 3",           "Dubai", "Dubai", Domain.AreaType.District,   951);
        Add("Arabian Ranches",        "Dubai", "Dubai", Domain.AreaType.Community,  950);
        Add("Arabian Ranches 2",      "Dubai", "Dubai", Domain.AreaType.Community,  949);
        Add("Arabian Ranches 3",      "Dubai", "Dubai", Domain.AreaType.Community,  948);
        Add("Motor City",             "Dubai", "Dubai", Domain.AreaType.Community,  946);
        Add("Sports City",            "Dubai", "Dubai", Domain.AreaType.Community,  944);
        Add("Dubai Silicon Oasis",    "Dubai", "Dubai", Domain.AreaType.FreeZone,   942);
        Add("Remraam",                "Dubai", "Dubai", Domain.AreaType.Community,  940);
        Add("International City",     "Dubai", "Dubai", Domain.AreaType.Community,  938);
        Add("Deira",                  "Dubai", "Dubai", Domain.AreaType.District,   936);
        Add("Bur Dubai",              "Dubai", "Dubai", Domain.AreaType.District,   934);
        Add("Karama",                 "Dubai", "Dubai", Domain.AreaType.District,   932);
        Add("Al Quoz",                "Dubai", "Dubai", Domain.AreaType.District,   930);
        Add("Al Qusais",              "Dubai", "Dubai", Domain.AreaType.District,   928);
        Add("Mirdif",                 "Dubai", "Dubai", Domain.AreaType.Community,  926);
        Add("Rashidiya",              "Dubai", "Dubai", Domain.AreaType.Community,  924);
        Add("Discovery Gardens",      "Dubai", "Dubai", Domain.AreaType.Community,  922);
        Add("The Springs",            "Dubai", "Dubai", Domain.AreaType.Community,  920);
        Add("The Meadows",            "Dubai", "Dubai", Domain.AreaType.Community,  918);
        Add("The Lakes",              "Dubai", "Dubai", Domain.AreaType.Community,  916);
        Add("The Greens",             "Dubai", "Dubai", Domain.AreaType.Community,  914);
        Add("Emirates Hills",         "Dubai", "Dubai", Domain.AreaType.Community,  912);
        Add("Emirates Living",        "Dubai", "Dubai", Domain.AreaType.Community,  910);
        Add("Sobha Hartland",         "Dubai", "Dubai", Domain.AreaType.Community,  908);
        Add("MBR City",               "Dubai", "Dubai", Domain.AreaType.Community,  906);
        Add("Mohammed Bin Rashid City","Dubai","Dubai", Domain.AreaType.Community,  905);
        Add("Dubai Creek Harbour",    "Dubai", "Dubai", Domain.AreaType.Development, 904);
        Add("Dubai Investment Park",  "Dubai", "Dubai", Domain.AreaType.FreeZone,   902);
        Add("DIP",                    "Dubai", "Dubai", Domain.AreaType.FreeZone,   901);
        Add("Studio City",            "Dubai", "Dubai", Domain.AreaType.Community,  900);
        Add("Dubai Studio City",      "Dubai", "Dubai", Domain.AreaType.Community,  899);
        Add("Arjan",                  "Dubai", "Dubai", Domain.AreaType.Community,  898);
        Add("Damac Hills",            "Dubai", "Dubai", Domain.AreaType.Community,  896);
        Add("Damac Hills 2",          "Dubai", "Dubai", Domain.AreaType.Community,  894);
        Add("Villanova",              "Dubai", "Dubai", Domain.AreaType.Community,  892);
        Add("Mudon",                  "Dubai", "Dubai", Domain.AreaType.Community,  890);
        Add("Town Square",            "Dubai", "Dubai", Domain.AreaType.Community,  888);
        Add("Al Furjan",              "Dubai", "Dubai", Domain.AreaType.Community,  886);
        Add("Jumeirah Golf Estates",  "Dubai", "Dubai", Domain.AreaType.Community,  884);
        Add("Tilal Al Ghaf",          "Dubai", "Dubai", Domain.AreaType.Community,  882);
        Add("Dubai Land",             "Dubai", "Dubai", Domain.AreaType.Community,  880);
        Add("Dubailand",              "Dubai", "Dubai", Domain.AreaType.Community,  879);
        Add("Al Furjan",              "Dubai", "Dubai", Domain.AreaType.Community,  878);
        Add("Dubai Waterfront",       "Dubai", "Dubai", Domain.AreaType.Development, 876);
        Add("Mina Rashid",            "Dubai", "Dubai", Domain.AreaType.Development, 874);
        Add("Festival City",          "Dubai", "Dubai", Domain.AreaType.Community,  872);
        Add("Al Khail Heights",       "Dubai", "Dubai", Domain.AreaType.Community,  870);
        Add("The World Islands",      "Dubai", "Dubai", Domain.AreaType.Island,     868);
        Add("Trade Centre",           "Dubai", "Dubai", Domain.AreaType.District,   866);
        Add("Garhoud",                "Dubai", "Dubai", Domain.AreaType.District,   864);
        Add("Oud Metha",              "Dubai", "Dubai", Domain.AreaType.District,   862);
        Add("Dubai Healthcare City",  "Dubai", "Dubai", Domain.AreaType.FreeZone,   860);
        Add("Knowledge Village",      "Dubai", "Dubai", Domain.AreaType.FreeZone,   858);
        Add("Dubai Media City",       "Dubai", "Dubai", Domain.AreaType.FreeZone,   856);
        Add("Dubai Internet City",    "Dubai", "Dubai", Domain.AreaType.FreeZone,   854);
        Add("The Views",              "Dubai", "Dubai", Domain.AreaType.Community,  852);
        Add("Dubai Canal",            "Dubai", "Dubai", Domain.AreaType.Community,  850);
        Add("Naif",                   "Dubai", "Dubai", Domain.AreaType.District,   848);
        Add("Muteena",                "Dubai", "Dubai", Domain.AreaType.District,   846);
        Add("Satwa",                  "Dubai", "Dubai", Domain.AreaType.District,   844);
        Add("Zabeel",                 "Dubai", "Dubai", Domain.AreaType.District,   842);
        Add("Za'abeel",               "Dubai", "Dubai", Domain.AreaType.District,   841);
        Add("Ras Al Khor",            "Dubai", "Dubai", Domain.AreaType.District,   840);
        Add("Al Warsan",              "Dubai", "Dubai", Domain.AreaType.District,   838);
        Add("Al Muhaisnah",           "Dubai", "Dubai", Domain.AreaType.District,   836);
        Add("Burj Khalifa District",  "Dubai", "Dubai", Domain.AreaType.Community,  834);
        Add("IMPZ",                   "Dubai", "Dubai", Domain.AreaType.FreeZone,   832);
        Add("Green Community",        "Dubai", "Dubai", Domain.AreaType.Community,  830);
        Add("Al Mamzar",              "Dubai", "Dubai", Domain.AreaType.District,   828);
        Add("Port Rashid",            "Dubai", "Dubai", Domain.AreaType.District,   826);
        Add("Al Quoz Industrial",     "Dubai", "Dubai", Domain.AreaType.Industrial, 824);
        Add("Dubai Production City",  "Dubai", "Dubai", Domain.AreaType.FreeZone,   822);
        Add("Jumeirah Park",          "Dubai", "Dubai", Domain.AreaType.Community,  820);
        Add("Jumeirah Islands",       "Dubai", "Dubai", Domain.AreaType.Community,  818);
        Add("The Palm Jumeirah",      "Dubai", "Dubai", Domain.AreaType.Island,     816);
        Add("Palm Views",             "Dubai", "Dubai", Domain.AreaType.Development, 814);
        Add("Emaar Beachfront",       "Dubai", "Dubai", Domain.AreaType.Development, 812);
        Add("Dubai Creek",            "Dubai", "Dubai", Domain.AreaType.District,   810);
        Add("Hatta",                  "Dubai", "Dubai", Domain.AreaType.District,   808);
        Add("Al Barari",              "Dubai", "Dubai", Domain.AreaType.Community,  806);
        Add("Akoya Oxygen",           "Dubai", "Dubai", Domain.AreaType.Community,  804);
        Add("Palm Hills",             "Dubai", "Dubai", Domain.AreaType.Community,  802);
        Add("Victory Heights",        "Dubai", "Dubai", Domain.AreaType.Community,  800);
        Add("Wadi Al Safa",           "Dubai", "Dubai", Domain.AreaType.Community,  798);
        Add("Academic City",          "Dubai", "Dubai", Domain.AreaType.FreeZone,   796);
        Add("International Media Production Zone", "Dubai","Dubai", Domain.AreaType.FreeZone, 794);

        // ── SHARJAH ──────────────────────────────────────────────────────
        Add("Sharjah City",           "Sharjah", "Sharjah", Domain.AreaType.City,       990);
        Add("Al Nahda",               "Sharjah", "Sharjah", Domain.AreaType.District,   988);
        Add("Al Qasimia",             "Sharjah", "Sharjah", Domain.AreaType.District,   986);
        Add("Al Taawun",              "Sharjah", "Sharjah", Domain.AreaType.District,   984);
        Add("Al Majaz",               "Sharjah", "Sharjah", Domain.AreaType.District,   982);
        Add("Al Majaz 1",             "Sharjah", "Sharjah", Domain.AreaType.District,   981);
        Add("Al Majaz 2",             "Sharjah", "Sharjah", Domain.AreaType.District,   980);
        Add("Al Majaz 3",             "Sharjah", "Sharjah", Domain.AreaType.District,   979);
        Add("Al Khan",                "Sharjah", "Sharjah", Domain.AreaType.District,   978);
        Add("Al Mamzar Sharjah",      "Sharjah", "Sharjah", Domain.AreaType.District,   976);
        Add("Al Nud",                 "Sharjah", "Sharjah", Domain.AreaType.District,   974);
        Add("Al Ramla",               "Sharjah", "Sharjah", Domain.AreaType.District,   972);
        Add("Muwaileh",               "Sharjah", "Sharjah", Domain.AreaType.Community,  970);
        Add("Al Zahia",               "Sharjah", "Sharjah", Domain.AreaType.Community,  968);
        Add("Aljada",                 "Sharjah", "Sharjah", Domain.AreaType.Community,  966);
        Add("Tilal City",             "Sharjah", "Sharjah", Domain.AreaType.Community,  964);
        Add("Sharjah Waterfront City","Sharjah", "Sharjah", Domain.AreaType.Community,  962);
        Add("Al Saja'a Industrial",   "Sharjah", "Sharjah", Domain.AreaType.Industrial, 960);
        Add("Hamriyah Free Zone",     "Sharjah", "Sharjah", Domain.AreaType.FreeZone,   958);
        Add("Sharjah Airport Free Zone","Sharjah","Sharjah", Domain.AreaType.FreeZone,  956);
        Add("Al Dhaid",               "Sharjah", "Al Dhaid", Domain.AreaType.City,      954);
        Add("Khor Fakkan",            "Sharjah", "Khor Fakkan", Domain.AreaType.City,   952);
        Add("Kalba",                  "Sharjah", "Kalba",   Domain.AreaType.City,       950);
        Add("Dibba Al Hisn",          "Sharjah", "Dibba",   Domain.AreaType.City,       948);
        Add("Khorfakkan Industrial",  "Sharjah", "Khor Fakkan", Domain.AreaType.Industrial, 946);
        Add("Al Jurf",                "Sharjah", "Sharjah", Domain.AreaType.District,   944);
        Add("Bu Tina",                "Sharjah", "Sharjah", Domain.AreaType.District,   942);
        Add("Al Yarmuk",              "Sharjah", "Sharjah", Domain.AreaType.District,   940);
        Add("Maysaloon",              "Sharjah", "Sharjah", Domain.AreaType.District,   938);
        Add("Al Gharb",               "Sharjah", "Sharjah", Domain.AreaType.District,   936);
        Add("Rolla",                  "Sharjah", "Sharjah", Domain.AreaType.District,   934);

        // ── AJMAN ────────────────────────────────────────────────────────
        Add("Ajman City",             "Ajman", "Ajman", Domain.AreaType.City,         990);
        Add("Al Nuaimia",             "Ajman", "Ajman", Domain.AreaType.District,     988);
        Add("Al Rashidiya",           "Ajman", "Ajman", Domain.AreaType.District,     986);
        Add("Al Hamidiyah",           "Ajman", "Ajman", Domain.AreaType.District,     984);
        Add("Al Jurf Ajman",          "Ajman", "Ajman", Domain.AreaType.District,     982);
        Add("Al Helio",               "Ajman", "Ajman", Domain.AreaType.District,     980);
        Add("Al Rawdah Ajman",        "Ajman", "Ajman", Domain.AreaType.District,     978);
        Add("Al Yasmeen",             "Ajman", "Ajman", Domain.AreaType.Community,    976);
        Add("Al Mowaihat",            "Ajman", "Ajman", Domain.AreaType.District,     974);
        Add("Al Khor Ajman",          "Ajman", "Ajman", Domain.AreaType.District,     972);
        Add("Ajman Uptown",           "Ajman", "Ajman", Domain.AreaType.Community,    970);
        Add("Emirates City",          "Ajman", "Ajman", Domain.AreaType.Community,    968);
        Add("Al Tallah 1",            "Ajman", "Ajman", Domain.AreaType.Community,    966);
        Add("Al Tallah 2",            "Ajman", "Ajman", Domain.AreaType.Community,    964);
        Add("Al Jurf Industrial",     "Ajman", "Ajman", Domain.AreaType.Industrial,   962);
        Add("Ajman Free Zone",        "Ajman", "Ajman", Domain.AreaType.FreeZone,     960);
        Add("Al Sawan",               "Ajman", "Ajman", Domain.AreaType.District,     958);
        Add("Masfout",                "Ajman", "Ajman", Domain.AreaType.District,     956);

        // ── RAS AL KHAIMAH ───────────────────────────────────────────────
        Add("Ras Al Khaimah City",    "Ras Al Khaimah", "RAK City", Domain.AreaType.City,       990);
        Add("Al Hamra Village",       "Ras Al Khaimah", "RAK City", Domain.AreaType.Community,   988);
        Add("Mina Al Arab",           "Ras Al Khaimah", "RAK City", Domain.AreaType.Community,   986);
        Add("Marjan Island",          "Ras Al Khaimah", "RAK City", Domain.AreaType.Island,       984);
        Add("Hayat Island",           "Ras Al Khaimah", "RAK City", Domain.AreaType.Island,       982);
        Add("Al Nakheel RAK",         "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     980);
        Add("Al Muntasir",            "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     978);
        Add("Al Rams",                "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     976);
        Add("Al Dhait",               "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     974);
        Add("Khuzam",                 "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     972);
        Add("Al Jazira Al Hamra",     "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     970);
        Add("Al Mairid",              "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     968);
        Add("Dafan Al Nakheel",       "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     966);
        Add("Al Uraibi",              "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     964);
        Add("RAK Free Trade Zone",    "Ras Al Khaimah", "RAK City", Domain.AreaType.FreeZone,     962);
        Add("Wadi Al Qawasim",        "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     960);
        Add("Al Jazirah Al Hamra",    "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     958);
        Add("Al Kharran",             "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     956);
        Add("Khatt",                  "Ras Al Khaimah", "RAK City", Domain.AreaType.District,     954);

        // ── FUJAIRAH ──────────────────────────────────────────────────────
        Add("Fujairah City",          "Fujairah", "Fujairah", Domain.AreaType.City,     990);
        Add("Dibba Al Fujairah",      "Fujairah", "Dibba",    Domain.AreaType.City,     988);
        Add("Kalba Fujairah",         "Fujairah", "Kalba",    Domain.AreaType.City,     986);
        Add("Masafi",                 "Fujairah", "Fujairah", Domain.AreaType.District,  984);
        Add("Qidfa",                  "Fujairah", "Fujairah", Domain.AreaType.District,  982);
        Add("Al Bidyah",              "Fujairah", "Fujairah", Domain.AreaType.District,  980);
        Add("Al Aqah",                "Fujairah", "Fujairah", Domain.AreaType.District,  978);
        Add("Mirbah",                 "Fujairah", "Fujairah", Domain.AreaType.District,  976);
        Add("Fujairah Free Zone",     "Fujairah", "Fujairah", Domain.AreaType.FreeZone,  974);

        // ── UMM AL QUWAIN ────────────────────────────────────────────────
        Add("Umm Al Quwain City",     "Umm Al Quwain", "UMQ", Domain.AreaType.City,     990);
        Add("Al Salama UMQ",          "Umm Al Quwain", "UMQ", Domain.AreaType.District,  988);
        Add("Al Raas",                "Umm Al Quwain", "UMQ", Domain.AreaType.District,  986);
        Add("Al Khor UMQ",            "Umm Al Quwain", "UMQ", Domain.AreaType.District,  984);
        Add("Al Abraq",               "Umm Al Quwain", "UMQ", Domain.AreaType.District,  982);
        Add("UMQ Free Trade Zone",    "Umm Al Quwain", "UMQ", Domain.AreaType.FreeZone,  980);

        db.UaeAreas.AddRange(areas);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} UAE areas.", areas.Count);
    }
}
