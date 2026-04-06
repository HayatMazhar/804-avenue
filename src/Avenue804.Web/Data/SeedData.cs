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
            (ContentBlockSlugs.HomeHeroEyebrow, null, "Abu Dhabi's Trusted Property &amp; Contracting Firm"),
            (ContentBlockSlugs.HomeHeroSubtitle, null,
                "From luxury property listings to complete construction, fit-out, and maintenance — 804 Avenue is your single trusted partner across the UAE."),
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
}
