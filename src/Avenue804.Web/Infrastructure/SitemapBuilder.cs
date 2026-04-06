using System.Security;
using System.Text;
using Avenue804.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Infrastructure;

public static class SitemapBuilder
{
    private static readonly string[] StaticPaths =
    {
        "/",
        "/Properties",
        "/Projects",
        "/Contracting",
        "/Maintenance",
        "/FacilityManagement",
        "/About",
        "/Contact",
        "/Privacy"
    };

    public static async Task<string> BuildAsync(ApplicationDbContext db, string baseUrl, CancellationToken cancellationToken = default)
    {
        var root = baseUrl.TrimEnd('/');
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        foreach (var path in StaticPaths)
            AppendUrl(sb, root + path, "weekly", path == "/" ? "1.0" : "0.8");

        var propertySlugs = await db.PropertyListings.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .Select(p => p.Slug!)
            .ToListAsync(cancellationToken);
        foreach (var slug in propertySlugs)
            AppendUrl(sb, $"{root}/Properties/{slug}", "weekly", "0.7");

        var projectSlugs = await db.PortfolioProjects.AsNoTracking()
            .Where(p => p.IsPublished && p.Slug != null && p.Slug != "")
            .Select(p => p.Slug!)
            .ToListAsync(cancellationToken);
        foreach (var slug in projectSlugs)
            AppendUrl(sb, $"{root}/Projects/{slug}", "monthly", "0.7");

        sb.AppendLine("</urlset>");
        return sb.ToString();
    }

    private static void AppendUrl(StringBuilder sb, string loc, string changefreq, string priority)
    {
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{SecurityElement.Escape(loc)}</loc>");
        sb.AppendLine($"    <changefreq>{changefreq}</changefreq>");
        sb.AppendLine($"    <priority>{priority}</priority>");
        sb.AppendLine("  </url>");
    }
}
