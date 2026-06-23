using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Avenue804.Web.Services;

public sealed class HomeStatsService : IHomeStatsService
{
    public static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private const string CacheKey = "home:stats:v1";

    public void InvalidateCache() => _cache.Remove(CacheKey);
    private const int FallbackFoundedYear = 2010;

    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public HomeStatsService(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<HomeStats> GetAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out HomeStats? cached) && cached is not null)
            return cached;

        // EF Core forbids concurrent operations on the same DbContext, so each
        // count query is awaited sequentially. Results are cheap (1h cached) and
        // the four queries run in single-digit milliseconds combined.
        var projectsCount = await _db.PortfolioProjects.AsNoTracking()
            .CountAsync(p => p.IsPublished, ct);

        var resolvedCount = await _db.MaintenanceTickets.AsNoTracking()
            .CountAsync(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed, ct);

        // Visibility is driven solely by IsPublished — the approval workflow
        // (ApprovalStatus) only gates user/agent-submitted listings before an
        // admin publishes them, and must not hide admin-published inventory.
        var listingsCount = await _db.PropertyListings.AsNoTracking()
            .CountAsync(p => p.IsPublished, ct);

        var foundedYearStr = await _db.SiteSettings.AsNoTracking()
            .Where(s => s.Key == "founded_year")
            .Select(s => s.Value)
            .FirstOrDefaultAsync(ct);

        var foundedYear = int.TryParse(foundedYearStr, out var fy) ? fy : FallbackFoundedYear;
        var years = Math.Max(1, DateTime.UtcNow.Year - foundedYear);

        var stats = new HomeStats(
            ProjectsDelivered: projectsCount,
            MaintenanceTicketsResolved: resolvedCount,
            PropertiesListed: listingsCount,
            YearsExperience: years);

        _cache.Set(CacheKey, stats, CacheTtl);
        return stats;
    }
}
