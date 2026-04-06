using Avenue804.Web.Data;
using Avenue804.Web.Domain;
using Microsoft.EntityFrameworkCore;

namespace Avenue804.Web.Services;

public class LookupService : ILookupService
{
    private readonly ApplicationDbContext _db;

    public LookupService(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<LookupValue>> GetActiveValuesAsync(string categoryCode, CancellationToken cancellationToken = default)
    {
        var code = categoryCode.Trim();
        return await _db.LookupValues.AsNoTracking()
            .Include(v => v.Category)
            .Where(v => v.IsActive && v.Category != null && v.Category.Code == code)
            .OrderBy(v => v.SortOrder)
            .ThenBy(v => v.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<LookupValue?> FindValueAsync(int id, string categoryCode, CancellationToken cancellationToken = default)
    {
        var code = categoryCode.Trim();
        return await _db.LookupValues.AsNoTracking()
            .Include(v => v.Category)
            .FirstOrDefaultAsync(
                v => v.Id == id && v.IsActive && v.Category != null && v.Category.Code == code,
                cancellationToken);
    }
}
