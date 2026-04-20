namespace Avenue804.Web.Services;

/// <summary>
/// Computes the four headline counters shown in the home-page stats bar from
/// real database aggregates (no hard-coded marketing numbers). Cached for
/// <see cref="HomeStatsService.CacheTtl"/> to keep the public landing page fast.
/// </summary>
public interface IHomeStatsService
{
    Task<HomeStats> GetAsync(CancellationToken ct = default);
}

/// <param name="ProjectsDelivered">Published portfolio projects.</param>
/// <param name="MaintenanceTicketsResolved">Resolved + Closed maintenance tickets.</param>
/// <param name="PropertiesListed">Published, approved property listings.</param>
/// <param name="YearsExperience">
/// Static "founded year" anchor — pulled from <c>SiteSettings.founded_year</c>
/// when present, otherwise falls back to a hard-coded sensible default.
/// </param>
public sealed record HomeStats(
    int ProjectsDelivered,
    int MaintenanceTicketsResolved,
    int PropertiesListed,
    int YearsExperience);
