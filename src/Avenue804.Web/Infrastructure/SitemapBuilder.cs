using System.Security;
using System.Text;
using Avenue804.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Infrastructure;

public static class SitemapBuilder
{
    private static readonly (string Path, string Freq, string Priority)[] StaticPaths =
    [
        ("/", "weekly", "1.0"),
        ("/Properties", "daily", "0.9"),
        ("/Properties/OffPlan", "weekly", "0.9"),
        ("/MarketTrends", "weekly", "0.8"),
        ("/HomeValuation", "weekly", "0.8"),
        ("/OurServices", "monthly", "0.8"),
        ("/Contracting", "monthly", "0.8"),
        ("/Maintenance", "monthly", "0.8"),
        ("/FacilityManagement", "monthly", "0.8"),
        ("/About", "monthly", "0.7"),
        ("/Contact", "monthly", "0.7"),
        ("/AreaGuides", "weekly", "0.8"),
        ("/Projects", "monthly", "0.7"),
        ("/Privacy", "yearly", "0.3"),
    ];

    public static async Task<string> BuildAsync(ApplicationDbContext db, string baseUrl, CancellationToken cancellationToken = default)
    {
        var root = baseUrl.TrimEnd('/');
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var (path, freq, prio) in StaticPaths)
            AppendUrl(sb, root + path, freq, prio);

        // Property listings
        var propertySlugs = await db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .Select(p => new { p.Slug, p.UpdatedAt, p.CreatedAt })
            .ToListAsync(cancellationToken);
        foreach (var p in propertySlugs)
            AppendUrl(sb, $"{root}/Properties/{p.Slug}", "weekly", "0.8", p.UpdatedAt ?? p.CreatedAt);

        // Portfolio projects
        var projectSlugs = await db.PortfolioProjects.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .Select(p => new { p.Slug, p.CreatedAt })
            .ToListAsync(cancellationToken);
        foreach (var p in projectSlugs)
            AppendUrl(sb, $"{root}/Projects/{p.Slug}", "monthly", "0.7", p.CreatedAt);

        // Area guides
        var areaSlugs = await db.AreaGuides.AsNoTracking()
            .Where(g => g.IsPublished)
            .Select(g => new { g.Slug, g.UpdatedAt })
            .ToListAsync(cancellationToken);
        foreach (var g in areaSlugs)
            AppendUrl(sb, $"{root}/AreaGuides/{g.Slug}", "weekly", "0.8", g.UpdatedAt);

        // Developer profiles
        var devSlugs = await db.Developers.AsNoTracking()
            .Where(d => d.IsActive && d.Slug != null)
            .Select(d => d.Slug!)
            .ToListAsync(cancellationToken);
        foreach (var slug in devSlugs)
            AppendUrl(sb, $"{root}/Developers/{slug}", "weekly", "0.7");

        // Agent profiles
        var agentIds = await db.Agents.AsNoTracking()
            .Where(a => a.IsActive)
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);
        foreach (var id in agentIds)
            AppendUrl(sb, $"{root}/Agents/{id}", "weekly", "0.7");

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    private static void AppendUrl(StringBuilder sb, string loc, string changefreq, string priority, DateTimeOffset? lastmod = null)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{SecurityElement.Escape(loc)}</loc>");
        if (lastmod.HasValue)
            sb.AppendLine($"    <lastmod>{lastmod.Value:yyyy-MM-dd}</lastmod>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine("  </url>");
    }
}
