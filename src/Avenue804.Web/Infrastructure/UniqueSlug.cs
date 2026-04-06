using Avenue804.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Infrastructure;

public static class UniqueSlug
{
    private const int MaxLen = 320;

    public static async Task<string> ForPropertyListingAsync(
        ApplicationDbContext db,
        string baseSlug,
        int? excludeId,
        CancellationToken cancellationToken = default)
    {
        return await AllocateAsync(
            baseSlug,
            async candidate =>
            {
                var q = db.PropertyListings.Where(p => p.Slug == candidate);
                if (excludeId.HasValue)
                    q = q.Where(p => p.Id != excludeId.Value);
                return !await q.AnyAsync(cancellationToken);
            },
            cancellationToken);
    }

    public static async Task<string> ForPortfolioProjectAsync(
        ApplicationDbContext db,
        string baseSlug,
        int? excludeId,
        CancellationToken cancellationToken = default)
    {
        return await AllocateAsync(
            baseSlug,
            async candidate =>
            {
                var q = db.PortfolioProjects.Where(p => p.Slug == candidate);
                if (excludeId.HasValue)
                    q = q.Where(p => p.Id != excludeId.Value);
                return !await q.AnyAsync(cancellationToken);
            },
            cancellationToken);
    }

    private static async Task<string> AllocateAsync(
        string baseSlug,
        Func<string, Task<bool>> isAvailable,
        CancellationToken cancellationToken)
    {
        var root = TrimToMax(baseSlug, MaxLen);
        if (string.IsNullOrEmpty(root))
            root = "item";

        for (var i = 0; i < 10_000; i++)
        {
            string candidate;
            if (i == 0)
            {
                candidate = root;
            }
            else
            {
                var suffix = $"-{i + 1}";
                var maxRoot = MaxLen - suffix.Length;
                var trimmed = maxRoot > 0 ? TrimToMax(root, maxRoot).TrimEnd('-') : "x";
                if (string.IsNullOrEmpty(trimmed))
                    trimmed = "x";
                candidate = trimmed + suffix;
            }

            if (await isAvailable(candidate))
                return candidate;
        }

        throw new InvalidOperationException("Could not allocate a unique slug.");
    }

    private static string TrimToMax(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max)
            return s;
        return s[..max].TrimEnd('-');
    }
}
